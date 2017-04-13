/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Zongsoft.Web.Http
{
	public class ExceptionFilter : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			if(actionExecutedContext.Exception == null)
				return;

			if(actionExecutedContext.Exception is Zongsoft.Security.Membership.AuthenticationException ||
			   actionExecutedContext.Exception is Zongsoft.Security.Membership.AuthorizationException)
			{
				actionExecutedContext.Response = this.GetExceptionResponse(actionExecutedContext.Exception, HttpStatusCode.Forbidden);

				//退出，不用记录日志
				return;
			}

			//生成返回的异常消息内容
			actionExecutedContext.Response = this.GetExceptionResponse(actionExecutedContext.Exception);

			//默认将异常信息写入日志文件
			Zongsoft.Diagnostics.Logger.Error(actionExecutedContext.Exception);
		}

		private HttpResponseMessage GetExceptionResponse(Exception exception, HttpStatusCode status = HttpStatusCode.InternalServerError)
		{
			if(exception == null)
				return null;

			var message = exception.Message;

			if(exception.InnerException != null)
				message += Environment.NewLine + exception.InnerException.Message;

			message = message?.Replace('"', '\'');

			var response = new HttpResponseMessage(status)
			{
				Content = new StringContent($"{{\"type\":\"{exception.GetType().Name}\",\"message\":\"{message}\"}}", System.Text.Encoding.UTF8, "application/json")
			};

			return response;
		}
	}
}
