﻿using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Customer.Model;
using Zapier.IntegrationModule.Web.Providers.Interfaces;

namespace Zapier.IntegrationModule.Web.Controllers.Api
{
    [AllowAnonymous]
    [RoutePrefix("api/zapier")]
    public class ActionController : ApiController
    {
        private readonly IContactsProvider _contactsProvider;

        public ActionController(IContactsProvider contactsProvider)
        {
            _contactsProvider = contactsProvider;
        }

        [HttpPost]
        [ResponseType(typeof(Contact))]
        [Route("contact")]
        public IHttpActionResult CreateContact(Contact contact)
        {
            var retVal = _contactsProvider.NewContact(contact);
            return Ok(retVal);
        }
    }
}
