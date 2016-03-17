/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
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

		public virtual User Post(User model)
		{
			if(model == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			string password = null;
			IEnumerable<string> values;

			//从请求消息的头部获取指定的用户密码
			if(this.Request.Headers.TryGetValues("x-password", out values))
				password = values.FirstOrDefault();

			if(this.UserProvider.CreateUser(model, password))
				return model;

			throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
		}

		[HttpPatch, HttpPut]
		public virtual async Task<int> Patch(int id, string args = null)
		{
			IDictionary<string, object> dictionary = null;

			if(this.Request.Content.IsFormData())
			{
				var form = await this.Request.Content.ReadAsFormDataAsync();

				if(form != null && form.Count > 0)
				{
					dictionary = new Dictionary<string, object>(form.Count, StringComparer.OrdinalIgnoreCase);

					foreach(var key in form.AllKeys)
					{
						dictionary[key] = form[key];
					}
				}
			}
			else if(this.Request.Content.IsMimeMultipartContent())
			{
				var multipart = await this.Request.Content.ReadAsMultipartAsync();

				if(multipart != null && multipart.Contents.Count > 0)
				{
					dictionary = new Dictionary<string, object>(multipart.Contents.Count, StringComparer.OrdinalIgnoreCase);

					foreach(StreamContent content in multipart.Contents)
					{
						var disposition = content.Headers.ContentDisposition;

						if(string.IsNullOrWhiteSpace(disposition.FileName))
							dictionary[disposition.Name.Trim('"', '\'')] = await content.ReadAsStringAsync();
						else
							throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
					}
				}
			}
			else if(string.Equals(this.Request.Content.Headers.ContentType.MediaType, "text/json", StringComparison.OrdinalIgnoreCase) ||
				    string.Equals(this.Request.Content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
			{
				var text = await this.Request.Content.ReadAsStringAsync();

				if(!string.IsNullOrWhiteSpace(text))
					dictionary = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<Dictionary<string, object>>(text);
			}

			if(!string.IsNullOrWhiteSpace(args))
			{
				var parts = args.Split('=', ':');

				if(string.IsNullOrWhiteSpace(parts[0]))
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

				if(dictionary == null)
					dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
								 {
									{ parts[0].Trim(), Uri.UnescapeDataString(parts[1]) },
								 };
				else
					dictionary[parts[0].Trim()] = Uri.UnescapeDataString(parts[1]);
			}

			if(dictionary == null || dictionary.Count == 0)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			int count = 0;

			foreach(var entry in dictionary)
			{
				if(string.IsNullOrWhiteSpace(entry.Key))
					continue;

				switch(entry.Key.ToLowerInvariant())
				{
					case "name":
						count += this.UserProvider.SetName(id, (string)entry.Value) ? 1 : 0;
						break;
					case "fullname":
						count += this.UserProvider.SetFullName(id, (string)entry.Value) ? 1 : 0;
						break;
					case "email":
						count += this.UserProvider.SetEmail(id, (string)entry.Value) ? 1 : 0;
						break;
					case "phone":
					case "phonenumber":
						count += this.UserProvider.SetPhoneNumber(id, (string)entry.Value) ? 1 : 0;
						break;
					case "avatar":
						count += this.UserProvider.SetAvatar(id, (string)entry.Value) ? 1 : 0;
						break;
					case "principal":
						count += this.UserProvider.SetPrincipalId(id, (string)entry.Value) ? 1 : 0;
						break;
					case "description":
						count += this.UserProvider.SetDescription(id, (string)entry.Value) ? 1 : 0;
						break;
				}
			}

			return count;
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
		public virtual void Approve(int id, string args)
		{
			var approved = string.IsNullOrWhiteSpace(args) ? true : Zongsoft.Common.Convert.ConvertValue<bool>(args, true);

			if(!this.UserProvider.Approve(id, approved))
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpPut]
		public virtual void Suspend(int id, string args)
		{
			var suspended = string.IsNullOrWhiteSpace(args) ? true : Zongsoft.Common.Convert.ConvertValue<bool>(args, true);

			if(!this.UserProvider.Suspend(id, suspended))
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpPut]
		public virtual void ChangePassword(int id, [Zongsoft.Web.Http.FromContent]string oldPassword, [Zongsoft.Web.Http.FromContent]string newPassword)
		{
			if(!this.UserProvider.ChangePassword(id, oldPassword, newPassword))
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
		}
		#endregion
	}
}
