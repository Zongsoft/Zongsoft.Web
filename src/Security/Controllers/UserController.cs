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
		/// <summary>
		/// 查询指定编号或用户标识、命名空间的用户。
		/// </summary>
		/// <param name="id">指定的路由参数，如果该参数为纯数字则会被当做为用户编号；否则请参考备注部分的处理规则。</param>
		/// <param name="paging">指定的查询分页设置。</param>
		/// <returns>返回的用户或用户集。</returns>
		/// <remarks>
		///		<para>注意：由于路由匹配约定，对于首字符为字母并且中间字符为字母、数字、下划线的路由数据并不会被匹配为<paramref name="id"/>，
		///		因此对于查询用户标识和命名空间的组合条件，该参数应该使用冒号进行组合；而对于查询指定命名空间内的所有用户则应以冒号打头，大致示意如下：</para>
		///		<list type="bullet">
		///			<listheader>
		///				<term>URL</term>
		///				<description>备注</description>
		///			</listheader>
		///			<item>
		///				<term>/api/Security/User/101</term>
		///				<description>查询用户<seealso cref="User.UserId"/>为101的用户。</description>
		///			</item>
		///			<item>
		///				<term>/api/Security/User/admin:zongsoft</term>
		///				<description>查询用户<seealso cref="User.Name"/>为：admin，且<seealso cref="User.Namespace"/>为：zongsoft的用户。</description>
		///			</item>
		///			<item>
		///				<term>/api/Security/User/13812345678:zongsoft</term>
		///				<description>查询用户<seealso cref="User.PhoneNumber"/>为：13812345678，且<seealso cref="User.Namespace"/>为：zongsoft的用户。</description>
		///			</item>
		///			<item>
		///				<term>/api/Security/User/zongsoft@gmail.com:zongsoft</term>
		///				<description>查询用户<seealso cref="User.Email"/>为：zongsoft@gmail.com，且<seealso cref="User.Namespace"/>为：zongsoft的用户。</description>
		///			</item>
		///			<item>
		///				<term>/api/Security/User/:zongsoft</term>
		///				<description>查询<seealso cref="User.Namespace"/>为：zongsoft的所有用户。</description>
		///			</item>
		///			<item>
		///				<term>/api/Security/User/@zongsoft</term>
		///				<description>查询<seealso cref="User.Namespace"/>为：zongsoft的所有用户。</description>
		///			</item>
		///		</list>
		/// </remarks>
		[Zongsoft.Web.Http.HttpPaging]
		public virtual object Get(string id = null, [FromUri]Paging paging = null)
		{
			if(string.IsNullOrWhiteSpace(id))
				return this.UserProvider.GetAllUsers(null, paging);

			//如果id参数以@符号打头，则将该参数作为命名空间进行查询
			if(id[0] == '@')
				return this.UserProvider.GetAllUsers(id.Substring(1), paging);

			int userId;

			//如果id参数为数字，则以用户编号的方式进行查询
			if(int.TryParse(id, out userId))
				return this.UserProvider.GetUser(userId);

			//注意：由于用户标识可能为邮箱，而邮箱地址中可能包含“-”横杠符和“@”，因此在此的分隔符只能采用冒号
			var parts = id.Split(':');

			if(parts.Length == 1)
				return this.UserProvider.GetAllUsers(id, paging);

			if(string.IsNullOrWhiteSpace(parts[0]))
				return this.UserProvider.GetAllUsers(parts[1], paging);
			else
				return this.UserProvider.GetUser(parts[0], parts[1]);
		}

		public virtual int Delete(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				return 0;

			int temp;
			var parts = id.Split(',', ';').Where(p => p.Length > 0 && int.TryParse(p, out temp)).Select(p => int.Parse(p)).ToArray();

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
		public virtual int Patch(int id, [Zongsoft.Web.Http.FromContent]IDictionary<string, object> content, string args = null)
		{
			if(!string.IsNullOrWhiteSpace(args))
			{
				var parts = args.Split('=', ':');

				if(string.IsNullOrWhiteSpace(parts[0]))
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

				if(content == null)
					content = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
								 {
									{ parts[0].Trim(), Uri.UnescapeDataString(parts[1]) },
								 };
				else
					content[parts[0].Trim()] = Uri.UnescapeDataString(parts[1]);
			}

			if(content == null || content.Count == 0)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			int count = 0;

			foreach(var entry in content)
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
		public virtual void Status(int id, string args)
		{
			if(string.IsNullOrWhiteSpace(args))
				return;

			var status = Zongsoft.Common.Convert.ConvertValue<UserStatus>(args);

			if(!this.UserProvider.SetStatus(id, status))
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
