using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web.Mvc;
using App.Client.Web.Models;
using App.Client.Web.Services;
using App.Domain.Contracts;

namespace App.Client.Web.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly ICustomerService _customerService;

        public CustomerController(
            IUserService userService,
            ICompanyService companyService,
            ICustomerService customerService,
            IFormsAuthenticationService formsAuthenticationService)
            : base(userService, formsAuthenticationService)
        {
            _companyService = companyService;
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            var customers = await _customerService.GetAll();
            return View(customers);
        }

        [HttpGet]
        public async Task<ViewResult> New()
        {
            var model = new CustomerCreateModel();

            var customFields = await _companyService.GetAllCustomFields(CurrentUser.CompanyId);
            model.CustomFields = customFields;

            model.Language = CurrentUser.Language;

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult New(CustomerCreateModel model)
        {
            if (!model.IsValid(model))
            {
                model.Msg = ViewBag.Txt["FailMsg"];
                return View(model);
            }

            var customerDto = new CustomerDto
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Password = model.Password,
                CompanyId = CurrentUser.CompanyId,
                CompanyName = CurrentUser.CompanyName,
                Language = CurrentUser.Language,
                CreatedBy = CurrentUser.Id,
                CustomFieldValues = new List<NameValueDto>()
            };

            var customerId = _customerService.CreateCustomer(customerDto);
            if (customerId == null)
            {
                model.Msg = ViewBag.Txt["FailMsg"];
                return View(model);
            }

            return RedirectToAction("Index", "Customer");
        }
    }
}