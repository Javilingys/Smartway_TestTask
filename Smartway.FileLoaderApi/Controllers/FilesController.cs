using LazyCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Smartway.FileLoaderApi.Contracts;
using Smartway.FileLoaderApi.Dtos;
using Smartway.FileLoaderApi.Entities;
using Smartway.FileLoaderApi.Extensions;
using System.Text;

namespace Smartway.FileLoaderApi.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;

    public FilesController(
        ILogger<FilesController> logger,
        UserManager<AppUser> userManager,
        IFileService fileService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userManager = userManager;
        _fileService = fileService;
        _unitOfWork = unitOfWork;
    }

    //1 Пользователь может одним запросом загрузить группу файлов (1...N).
    [HttpPost]
    public async Task<ActionResult> UploadFiles(IList<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("Нет файлов для загрузки.");
        }

        var userId = await _userManager.GetUserIdByEmailFromClaimsPrincipal(User);

        await _fileService.SaveFilesAsync(files, userId, cancellationToken);

        _logger.LogInformation("--> Закончилась загрузка файлов");

        return Ok();
    }

    // 2 Пользователь может посредством запроса узнать на сколько процентов загружен конкретный файл
    // или группа файлов в целом.
    [HttpGet("progress")]
    public async Task<ActionResult<LoadingProgressDto>> GetPercentageOfLoad([FromServices] IAppCache memoryCache)
    {
        var percentage = await memoryCache
            .GetAsync<int?>(await _userManager.GetUserIdByEmailFromClaimsPrincipal(User));

        return new LoadingProgressDto(percentage);
    }

    // 3 Пользователь может просмотреть список загруженных файлов или групп файлов.
    // П.С. Пользователь имеет доступ только к загруженным им файлам, если не указано обратное.
    [HttpGet]
    public async Task<ActionResult<List<AppFileDto>>> GetFilesForCurrentUser(bool canSeeAll = false)
    {
        var userId = await _userManager.GetUserIdByEmailFromClaimsPrincipal(User);

        if (userId is null)
        {
            return BadRequest("Ошибка получения пользователя.");
        }

        return await _unitOfWork.AppFileRepository.GetListOfLoadedFileDtosAsync(userId, canSeeAll);
    }

    // 4 Пользователь может одним запросом скачать файл или группу файлов.
    /// <summary>
    /// Загрузить файл или группу файлов <br></br>
    /// Должен присутсоввать или groupId или fieldId. Если передать оба, то поиск будет идти по группе (т.к. файл в этой группе и так)
    /// Если передать ничего, то BadRequest
    /// </summary>
    [HttpGet("download")]
    public async Task<ActionResult> DownloadFileOrGroup(Guid? groupId, int? fileId)
    {
        var userId = await _userManager.GetUserIdByEmailFromClaimsPrincipal(User);

        if (userId is null)
        {
            return BadRequest("Ошибка получения пользователя.");
        }

        if (!groupId.HasValue && !fileId.HasValue)
        {
            return BadRequest("Неверный запрос.");
        }

        var files = await _unitOfWork.AppFileRepository.GetFilesByGroupOrFileIdForUser(groupId, fileId, userId, asNoTracking: true);

        var zipFilePath = await _fileService.GenerateZipFileAsync(files);

        if (System.IO.File.Exists(zipFilePath))
        {
            var stream = System.IO.File.OpenRead(zipFilePath);
            return File(stream, "application/octet-stream", Path.GetFileName(zipFilePath));
        }
        else
        {
            return NotFound("ZIP-архив не найден.");
        }
    }

    [HttpPost("generate-one-time-link")]
    public async Task<ActionResult> GenerateOneTimeLink(Guid? groupId, int? fileId, [FromServices] ITokenService tokenService)
    {
        var userId = await _userManager.GetUserIdByEmailFromClaimsPrincipal(User);

        if (userId is null)
        {
            return BadRequest("Ошибка получения пользователя.");
        }

        if (!groupId.HasValue && !fileId.HasValue)
        {
            return BadRequest("Неверный запрос.");
        }

        var files = await _unitOfWork.AppFileRepository.GetFilesByGroupOrFileIdForUser(groupId, fileId, userId, asNoTracking: true);

        if (files.Count == 0 || !groupId.HasValue && files.Count > 1)
        {
            return BadRequest("Файлы не найдены.");
        }

        var token = tokenService.CreateTokenForOneTimeLink();

        var link = new OneTimeLink
        {
            Token = token,
            Expiry = DateTime.UtcNow.AddMinutes(1), // Set an expiry time, e.g., 1 hour
            WasUsed = false,
            GroupId = groupId.HasValue ? groupId.Value : files.First().GroupId
        };

        _unitOfWork.OneTimeLInkRepository.Add(link);
        await _unitOfWork.SaveChangesAsync();

        var tokenBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        // Construct the one-time link URL
        var linkUrl = Url.Action(nameof(GetByOneTimeLink), "Files", new { token = tokenBase64 }, Request.Scheme, Request.Host.Value);


        return Ok($"Ваша одноразовая ссылка: {linkUrl}");
    }

    [HttpGet("one-time/{token}")]
    public async Task<ActionResult> GetByOneTimeLink(string token)
    {
        token = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var oneTimeLink = await _unitOfWork.OneTimeLInkRepository.GetActiveByToken(token);

        if (oneTimeLink is null)
        {
            return BadRequest("Ссылка больше не действительна");
        }

        var appFiles = await _unitOfWork.AppFileRepository.GetFilesByGroupId(oneTimeLink.GroupId, asNoTracking: true);

        var zipFilePath = await _fileService.GenerateZipFileAsync(appFiles);

        if (System.IO.File.Exists(zipFilePath))
        {
            oneTimeLink.WasUsed = true;
            await _unitOfWork.SaveChangesAsync();

            var stream = System.IO.File.OpenRead(zipFilePath);
            return File(stream, "application/octet-stream", Path.GetFileName(zipFilePath));
        }
        else
        {
            return NotFound("ZIP-архив не найден.");
        }
    }
}

