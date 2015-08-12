﻿using DotLiquid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Web.Converters.Notifications;
using webModels = VirtoCommerce.Platform.Web.Model.Notifications;

namespace VirtoCommerce.Platform.Web.Controllers.Api
{
    [RoutePrefix("api/platform/notification")]
	public class NotificationsController : ApiController
	{
		private readonly INotificationTemplateService _notificationTemplateService;
		private readonly INotificationManager _notificationManager;
		private readonly INotificationTemplateResolver _eventTemplateResolver;
		public NotificationsController(
			INotificationTemplateService notificationTemplateService,
			INotificationManager notificationManager,
			INotificationTemplateResolver eventTemplateResolver)
		{
			_notificationTemplateService = notificationTemplateService;
			_notificationManager = notificationManager;
			_eventTemplateResolver = eventTemplateResolver;
		}

		/// <summary>
		/// Get all registered notification types
		/// </summary>
		/// <remarks>Get all registered notification types in platform</remarks>
		[HttpGet]
		[ResponseType(typeof(webModels.Notification[]))]
		[Route("")]
		public IHttpActionResult GetNotifications()
		{
			var notifications = _notificationManager.GetNotifications();

			return Ok(notifications.Select(s => s.ToWebModel()).ToArray());
		}


		/// <summary>
		/// Get notification template
		/// </summary>
		/// <param name="type">Notification type of template</param>
		/// <param name="objectId">Object id of template</param>
		/// <param name="objectTypeId">Object type id of template</param>
		/// <param name="language">Locale of template</param>
		/// <remarks>
		/// Get notification template by notification type, objectId, objectTypeId and language. Object id and object type id - params of object, that initialize creating of
		/// template. By default object id and object type id = "Platform". For example for store with id = "SampleStore", objectId = "SampleStore", objectTypeId = "Store".
		/// </remarks>
		[HttpGet]
		[ResponseType(typeof(webModels.NotificationTemplate))]
		[Route("template/{type}/{objectId}/{objectTypeId}/{language}")]
		public IHttpActionResult GetNotificationTemplate(string type, string objectId, string objectTypeId, string language)
		{
			NotificationTemplate retVal = new NotificationTemplate();
			var notification = _notificationManager.GetNewNotification(type, objectId, objectTypeId, language);
			if (notification != null)
			{
				retVal = notification.NotificationTemplate;
			}

			return Ok(retVal.ToWebModel());
		}

		/// <summary>
		/// Get notification templates
		/// </summary>
		/// <remarks>
		/// Get all notification templates by notification type, objectId, objectTypeId. Object id and object type id - params of object, that initialize creating of
		/// template. By default object id and object type id = "Platform". For example for store with id = "SampleStore", objectId = "SampleStore", objectTypeId = "Store".
		/// </remarks>
		/// <param name="type">Notification type of template</param>
		/// <param name="objectId">Object id of template</param>
		/// <param name="objectTypeId">Object type id of template</param>
		[HttpGet]
		[ResponseType(typeof(webModels.NotificationTemplate[]))]
		[Route("template/{type}/{objectId}/{objectTypeId}")]
        public IHttpActionResult GetNotificationTemplates(string type, string objectId, string objectTypeId)
		{
			List<webModels.NotificationTemplate> retVal = new List<webModels.NotificationTemplate>();
			var templates = _notificationTemplateService.GetNotificationTemplatesByNotification(type, objectId, objectTypeId);

            if (templates.Any())
			{
				retVal = templates.Select(t => t.ToWebModel()).ToList();
			}
			return Ok(retVal.ToArray());
		}

		/// <summary>
		/// Update notification template
		/// </summary>
		/// <param name="notificationTemplate">Notification template</param>
		[HttpPost]
		[ResponseType(typeof(void))]
		[Route("template")]
		public IHttpActionResult UpdateNotificationTemplate([FromBody] webModels.NotificationTemplate notificationTemplate)
		{
			_notificationTemplateService.Update(new NotificationTemplate[] { notificationTemplate.ToCoreModel() });

			return StatusCode(HttpStatusCode.NoContent);
		}

		/// <summary>
		/// Delete notification template
		/// </summary>
		/// <param name="id">Template id</param>
		[HttpDelete]
		[ResponseType(typeof(void))]
		[Route("template/{id}")]
		public IHttpActionResult DeleteNotificationTemplate(string id)
		{
			_notificationTemplateService.Delete(new string[] { id });

			return StatusCode(HttpStatusCode.NoContent);
		}

		/// <summary>
		/// Get testing parameters
		/// </summary>
		/// <remarks>Method returns notification properties, that defined in notification class, this proprties used in notification template.</remarks>
		/// <param name="type">Notification type</param>
		[HttpGet]
		[ResponseType(typeof(NotificationParameter[]))]
		[Route("template/{type}/getTestingParameters")]
		public IHttpActionResult GetTestingParameters(string type)
		{
			var notification = _notificationManager.GetNewNotification(type);
			var retVal = _eventTemplateResolver.ResolveNotificationParameters(notification);

			return Ok(retVal.Select(s => s.ToWebModel()).ToArray());
		}

		/// <summary>
		/// Get rendered notification html
		/// </summary>
		/// <remarks>
		/// Method returns rendered html, that based on notification template. Template for rendering choosen by type, objectId, objectTypeId, language.
		/// Parameters for template may be prepared by the method of getTestingParameters.
		/// </remarks>
		/// <param name="parameters">Notification special parameters</param>
		/// <param name="type">Notification type</param>
		/// <param name="objectId">Object id</param>
		/// <param name="objectTypeId">Object type id</param>
		/// <param name="language">Locale</param>
		[HttpPost]
		[ResponseType(typeof(webModels.Notification))]
		[Route("template/{type}/{objectId}/{objectTypeId}/{language}/rendernotificationhtml")]
		public IHttpActionResult ResolveNotification([FromBody]List<KeyValuePair<string, string>> parameters, string type, string objectId, string objectTypeId, string language)
		{
			var notification = _notificationManager.GetNewNotification(type, objectId, objectTypeId, language);
			foreach (var param in parameters)
			{
				var property = notification.GetType().GetProperty(param.Key);
				property.SetValue(notification, param.Value);
			}
			_eventTemplateResolver.ResolveTemplate(notification);

			return Ok(notification.ToWebModel());
		}

		/// <summary>
		/// Sending test notification
		/// </summary>
		/// <remarks>
		/// Method sending notification, that based on notification template. Template for rendering chosen by type, objectId, objectTypeId, language.
		/// Parameters for template may be prepared by the method of getTestingParameters. Method returns string. If sending finished with success status
		/// this string is empty, otherwise string contains error message.
		/// </remarks>
		/// <param name="parameters">Notification special parameters</param>
		/// <param name="type">Notification type</param>
		/// <param name="objectId">Object id</param>
		/// <param name="objectTypeId">Object type id</param>
		/// <param name="language">Locale</param>
		[HttpPost]
		[ResponseType(typeof(string))]
		[Route("template/{type}/{objectId}/{objectTypeId}/{language}/sendnotification")]
		public IHttpActionResult SendNotification([FromBody]List<KeyValuePair<string, string>> parameters, string type, string objectId, string objectTypeId, string language)
		{
			var notification = _notificationManager.GetNewNotification(type, objectId, objectTypeId, language);
			foreach (var param in parameters)
			{
				var property = notification.GetType().GetProperty(param.Key);
				property.SetValue(notification, param.Value);
			}
			var result = _notificationManager.SendNotification(notification);

			return Ok(result.ErrorMessage);
		}

		/// <summary>
		/// Get notification journal page
		/// </summary>
		/// <remarks>
		/// Method returns notification journal page with array of notification, that was send, sending or will be send in future. Result contains total count, that can be used
		/// for paging.
		/// </remarks>
		/// <param name="objectId">Object id</param>
		/// <param name="objectTypeId">Object type id</param>
		/// <param name="start">Page setting start</param>
		/// <param name="count">Page setting count</param>
		[HttpGet]
		[ResponseType(typeof(webModels.SearchNotificationsResult))]
		[Route("journal/{objectId}/{objectTypeId}")]
		public IHttpActionResult GetNotificationJournal(string objectId, string objectTypeId, int start, int count)
		{
			var result = _notificationManager.SearchNotifications(new SearchNotificationCriteria() { ObjectId = objectId, ObjectTypeId = objectTypeId, Skip = start, Take = count });

			var retVal = new webModels.SearchNotificationsResult();
			retVal.Notifications = result.Notifications.Select(nt => nt.ToWebModel()).ToArray();
			retVal.TotalCount = result.TotalCount;

			return Ok(retVal);
		}

		/// <summary>
		/// Get sending notification
		/// </summary>
		/// <param name="id">Sending notification id</param>
		[HttpGet]
		[ResponseType(typeof(webModels.Notification))]
		[Route("notification/{id}")]
		public IHttpActionResult GetNotification(string id)
		{
			var retVal = _notificationManager.GetNotificationById(id);

			return Ok(retVal.ToWebModel());
		}

		/// <summary>
		/// Stop sending notification
		/// </summary>
		/// <param name="ids">Stop sending notification ids</param>
		[HttpPost]
		[ResponseType(typeof(void))]
		[Route("stopnotifications")]
		public IHttpActionResult StopSendingNotifications(string[] ids)
		{
			_notificationManager.StopSendingNotifications(ids);

			return StatusCode(HttpStatusCode.NoContent);
		}
	}
}