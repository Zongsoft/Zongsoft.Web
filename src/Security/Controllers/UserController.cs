/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Web.
 *
 * Zongsoft.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;

using Zongsoft.Data;
using Zongsoft.Services;
using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Security.Controllers
{
	public class UserController : ApiController
	{
		#region 成员字段
		private IUserProvider _userProvider;
		#endregion

		#region 公共属性
		[ServiceDependency]
		public IUserProvider UserProvider
		{
			get
			{
				return _userProvider;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_userProvider = value;
			}
		}
		#endregion

		#region 公共方法
		[Zongsoft.Web.Http.HttpPaging]
		public virtual object Get(string id = null, [FromUri]Paging paging = null)
		{
			if(string.IsNullOrWhiteSpace(id))
				return this.UserProvider.GetAllUsers(null, paging);

			int userId;

			if(int.TryParse(id, out userId))
				return this.UserProvider.GetUser(userId);

			var parts = id.Split(':');

			if(parts.Length == 1)
				return this.UserProvider.GetAllUsers(id, paging);
			else
				return this.UserProvider.GetUser(parts[0], parts[1]);
		}

		public virtual int Delete(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				return 0;

			int temp;
			var parts = id.Split(',').Where(p => p.Length > 0 && int.TryParse(p, out temp)).Select(p => int.Parse(p)).ToArray();

			if(parts.Length > 0)
				return this.UserProvider.DeleteUsers(parts);

			return 0;
		}

		public virtual void Put(User model)
		{
			if(model == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			if(this.UserProvider.UpdateUsers(model) < 1)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		public virtual User Post(User model, string password = null)
		{
			if(model == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			if(this.UserProvider.CreateUser(model, password))
				return model;

			throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
		}

		[HttpPatch, HttpPut]
		public virtual void Patch(int id, string name, string value)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			bool succeed = false;

			switch(name.Trim().ToLowerInvariant())
			{
				case "name":
					succeed = this.UserProvider.SetName(id, value);
					break;
				case "fullname":
					succeed = this.UserProvider.SetFullName(id, value);
					break;
				case "email":
					succeed = this.UserProvider.SetEmail(id, value);
					break;
				case "phone":
				case "phonenumber":
					succeed = this.UserProvider.SetPhoneNumber(id, value);
					break;
				case "avatar":
					succeed = this.UserProvider.SetAvatar(id, value);
					break;
				case "principal":
					succeed = this.UserProvider.SetPrincipalId(id, value);
					break;
				case "description":
					succeed = this.UserProvider.SetDescription(id, value);
					break;
				default:
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
			}

			if(!succeed)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpGet]
		public virtual void Exists(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			var existed = false;
			int userId;

			if(int.TryParse(id, out userId))
			{
				existed = this.UserProvider.Exists(userId);
			}
			else
			{
				var parts = id.Split(':');

				if(parts.Length == 1)
					existed = this.UserProvider.Exists(parts[0], null);
				else
					existed = this.UserProvider.Exists(parts[0], parts[1]);
			}

			if(!existed)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpGet]
		public virtual void HasPassword(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			var existed = false;
			int userId;

			if(int.TryParse(id, out userId))
			{
				existed = this.UserProvider.HasPassword(userId);
			}
			else
			{
				var parts = id.Split(':');

				if(parts.Length == 1)
					existed = this.UserProvider.HasPassword(parts[0], null);
				else
					existed = this.UserProvider.HasPassword(parts[0], parts[1]);
			}

			if(!existed)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpPut]
		public virtual void Approve(int id, bool approved = true)
		{
			if(!this.UserProvider.Approve(id, approved))
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpPut]
		public virtual void Suspend(int id, bool suspended = true)
		{
			if(!this.UserProvider.Suspend(id, suspended))
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpPut]
		public virtual void ChangePassword(int id, string oldPassword, string newPassword)
		{
			if(!this.UserProvider.ChangePassword(id, oldPassword, newPassword))
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}
		#endregion
	}
}
