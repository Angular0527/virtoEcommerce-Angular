﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CustomerModule.Web.Converters;
using VirtoCommerce.CustomerModule.Web.Security;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Security;
using coreModel = VirtoCommerce.Domain.Customer.Model;
using webModel = VirtoCommerce.CustomerModule.Web.Model;

namespace VirtoCommerce.CustomerModule.Web.Controllers.Api
{
    [RoutePrefix("api")]
    [CheckPermission(Permission = CustomerPredefinedPermissions.Read)]
    public class CustomerModuleController : ApiController
    {
        private readonly IContactService _contactService;
        private readonly IOrganizationService _organizationService;
        private readonly ICustomerSearchService _contactSearchService;
        private readonly ISecurityService _securityService;

        public CustomerModuleController(IContactService contactService, IOrganizationService organizationService, ICustomerSearchService contactSearchService, ISecurityService securityService)
        {
            _contactSearchService = contactSearchService;
            _securityService = securityService;
            _organizationService = organizationService;
            _contactService = contactService;
        }

        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>Get array of all organizations.</remarks>
        [HttpGet]
        [ResponseType(typeof(webModel.Organization[]))]
        [Route("organizations")]
        public IHttpActionResult ListOrganizations()
        {
            var retVal = _organizationService.List().ToArray();
            return Ok(retVal);
        }

        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>Get array of members satisfied search criteria.</remarks>
        /// <param name="criteria">Search criteria</param>
        [HttpPost]
        [ResponseType(typeof(webModel.SearchResult))]
        [Route("members")]
        public IHttpActionResult Search(coreModel.SearchCriteria criteria)
        {
            var result = _contactSearchService.Search(criteria);

            var retVal = new webModel.SearchResult();

            var start = criteria.Skip;
            var count = criteria.Take;

            // all organizations
            var organizations = result.Organizations.Select(x => x.ToWebModel());
            var contacts = result.Contacts.Select(x => x.ToWebModel());

            var organizationsCount = organizations.Count();
            retVal.TotalCount = organizationsCount + result.TotalCount;
            retVal.Members.AddRange(organizations.Skip(start).Take(count));

            count -= organizationsCount;
            retVal.Members.AddRange(contacts.Take(count));

            return Ok(retVal);
        }


        /// <summary>
        /// Get contact
        /// </summary>
        /// <param name="id">Contact ID</param>
        [HttpGet]
        [ResponseType(typeof(webModel.Contact))]
        [Route("contacts/{id}")]
        public IHttpActionResult GetContactById(string id)
        {
            var retVal = _contactService.GetById(id);
            return retVal != null ? Ok(retVal.ToWebModel()) : (IHttpActionResult)Ok();
        }


        /// <summary>
        /// Create contact
        /// </summary>
        [HttpPost]
        [ResponseType(typeof(webModel.Contact))]
        [Route("contacts")]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateContact(webModel.Contact contact)
        {
            var retVal = _contactService.Create(contact.ToCoreModel());
            return Ok(retVal.ToWebModel());
        }

        /// <summary>
        /// Update contact
        /// </summary>
        /// <response code="204">Operation completed.</response>
        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("contacts")]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateContact(webModel.Contact contact)
        {
            _contactService.Update(new[] { contact.ToCoreModel() });
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>Delete contacts by given array of ids.</remarks>
        /// <param name="ids">An array of contacts ids</param>
        /// <response code="204">Operation completed.</response>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("contacts")]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteContacts([FromUri] string[] ids)
        {
            _contactService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }


        /// <summary>
        /// Get organization
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <response code="200"></response>
        /// <response code="404">Organization not found.</response>
        [HttpGet]
        [ResponseType(typeof(webModel.Organization))]
        [Route("organizations/{id}")]
        public IHttpActionResult GetOrganizationById(string id)
        {
            var retVal = _organizationService.GetById(id);
            return retVal != null ? Ok(retVal.ToWebModel()) : (IHttpActionResult)NotFound();
        }


        /// <summary>
        /// Create organization
        /// </summary>
        [HttpPost]
        [ResponseType(typeof(webModel.Organization))]
        [Route("organizations")]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateOrganization(webModel.Organization organization)
        {
            var retVal = _organizationService.Create(organization.ToCoreModel());
            return Ok(retVal.ToWebModel());
        }

        /// <summary>
        /// Update organization
        /// </summary>
        /// <response code="204">Operation completed.</response>
        [HttpPut]
        [ResponseType(typeof(void))]
        [Route("organizations")]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateOrganization(webModel.Organization organization)
        {
            _organizationService.Update(new[] { organization.ToCoreModel() });
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>Delete organizations by given array of ids.</remarks>
        /// <param name="ids">An array of organizations ids</param>
        /// <response code="204">Operation completed.</response>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("organizations")]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteOrganizations([FromUri] string[] ids)
        {
            _organizationService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

     
    }
}
