// Controllers/RouteController.cs
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.Controllers
{
    [Route("")]
    [Route("user")]
    [Route("apartment")]
    [Route("house")]
    [Route("notification")]
    [Route("event")]
    [Route("vote")]
    [Route("public-needs")]
    public class RouteController : Controller
    {
        private readonly ILogger<RouteController> _logger;

        public RouteController(ILogger<RouteController> logger)
        {
            _logger = logger;
        }

        #region User Routes

        [HttpGet("user/{id}")]
        public IActionResult UserProfile(string id)
        {
            _logger.LogInformation("Запрошена страница профиля пользователя с ID: {UserId}", id);
            ViewData["UserId"] = id;
            return View("User/UserProfile");
        }

        [HttpGet("user/create")]
        public IActionResult CreateUser()
        {
            _logger.LogInformation("Запрошена страница создания пользователя");
            return View("User/CreateUser");
        }

        #endregion

        #region Apartment Routes

        [HttpGet("apartment/by-user/{userId}")]
        public IActionResult ApartmentsByUser(string userId)
        {
            _logger.LogInformation("Запрошены квартиры для пользователя с ID: {UserId}", userId);
            ViewData["UserId"] = userId;
            return View("Apartment/ApartmentsByUser");
        }

        [HttpGet("apartment/{id}")]
        public IActionResult ApartmentDetails(string id)
        {
            _logger.LogInformation("Запрошена страница квартиры с ID: {ApartmentId}", id);
            ViewData["ApartmentId"] = id;
            return View("Apartment/ApartmentDetails");
        }

        [HttpGet("apartment/create")]
        public IActionResult CreateApartment()
        {
            _logger.LogInformation("Запрошена страница создания квартиры");
            return View("Apartment/CreateApartment");
        }

        [HttpGet("apartment/edit/{id}")]
        public IActionResult EditApartment()
        {
            _logger.LogInformation("Запрошена страница создания квартиры");
            return View("Apartment/EditApartment");
        }

        #endregion

        #region House Routes

        [HttpGet("house/{id}")]
        public IActionResult HouseDetails(string id)
        {
            _logger.LogInformation("Запрошена страница дома с ID: {HouseId}", id);
            ViewData["HouseId"] = id;
            return View("House/HouseDetails");
        }

        [HttpGet("house/create")]
        public IActionResult CreateHouse()
        {
            _logger.LogInformation("Запрошена страница создания дома");
            return View("House/CreateHouse");
        }

        [HttpGet("house/by-user/{userId}")]
        public IActionResult HousesByUser(string userId)
        {
            _logger.LogInformation("Запрошены дома для пользователя с ID: {UserId}", userId);
            ViewData["UserId"] = userId;
            return View("House/HouseByUser");
        }

        #endregion

        #region Notification Routes

        [HttpGet("notification/by-user/{userId}")]
        public IActionResult NotificationsByUser(string userId)
        {
            _logger.LogInformation("Запрошены уведомления для пользователя с ID: {UserId}", userId);
            ViewData["UserId"] = userId;
            return View("Notification/NotificationsByUser");
        }

        [HttpGet("notification/{id}")]
        public IActionResult NotificationDetails(string id)
        {
            _logger.LogInformation("Запрошено уведомление с ID: {NotificationId}", id);
            ViewData["NotificationId"] = id;
            return View("Notification/NotificationDetails");
        }

        [HttpGet("notification/create")]
        public IActionResult CreateNotification()
        {
            _logger.LogInformation("Запрошена страница создания уведомления");
            return View("Notification/CreateNotification");
        }

        #endregion

        #region Event Routes

        [HttpGet("event/by-user/{userId}")]
        public IActionResult EventsByUser(string userId)
        {
            _logger.LogInformation("Запрошены события для пользователя с ID: {UserId}", userId);
            ViewData["UserId"] = userId;
            return View("Event/EventsByUser");
        }

        [HttpGet("event/{id}")]
        public IActionResult EventDetails(string id)
        {
            _logger.LogInformation("Запрошено событие с ID: {EventId}", id);
            ViewData["EventId"] = id;
            return View("Event/EventDetails");
        }

        [HttpGet("event/create")]
        public IActionResult CreateEvent()
        {
            _logger.LogInformation("Запрошена страница создания события");
            return View("Event/CreateEvent");
        }

        #endregion

        #region Vote Routes

        [HttpGet("vote/by-user/{userId}")]
        public IActionResult VotesByUser(string userId)
        {
            _logger.LogInformation("Запрошены голосования для пользователя с ID: {UserId}", userId);
            ViewData["UserId"] = userId;
            return View("Vote/VotesByUser");
        }

        [HttpGet("vote/{id}")]
        public IActionResult VoteDetails(string id)
        {
            _logger.LogInformation("Запрошено голосование с ID: {VoteId}", id);
            ViewData["VoteId"] = id;
            return View("Vote/VoteDetails");
        }

        [HttpGet("vote/create")]
        public IActionResult CreateVote()
        {
            _logger.LogInformation("Запрошена страница создания голосования");
            return View("Vote/CreateVote");
        }

        #endregion

        #region Public Needs Routes

        [HttpGet("public-needs/by-user/{userId}")]
        public IActionResult PublicNeedsByUser(string userId)
        {
            _logger.LogInformation("Запрошены общественные нужды для пользователя с ID: {UserId}", userId);
            ViewData["UserId"] = userId;
            return View("PublicNeeds/PublicNeedsByUser");
        }

        [HttpGet("public-needs/{id}")]
        public IActionResult PublicNeedDetails(string id)
        {
            _logger.LogInformation("Запрошены общественные нужды с ID: {PublicNeedId}", id);
            ViewData["PublicNeedId"] = id;
            return View("PublicNeeds/PublicNeedDetails");
        }

        [HttpGet("public-needs/create")]
        public IActionResult CreatePublicNeed()
        {
            _logger.LogInformation("Запрошена страница создания общественных нужд");
            return View("PublicNeeds/CreatePublicNeed");
        }

        #endregion

        #region Default Route

        [HttpGet("")]
        public IActionResult Index()
        {
            _logger.LogInformation("Запрошена главная страница");
            return View("Home/Index");
        }

        #endregion
    }
}