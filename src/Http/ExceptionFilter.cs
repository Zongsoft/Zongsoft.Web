/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Zongsoft.Web.Http
{
	public class ExceptionFilter : ExceptionFilterAttribute
	{
		#region 重写方法
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			if(actionExecutedContext.Exception == null)
				return;

			if(actionExecutedContext.Exception is OperationCanceledException ex)
			{
				//如果操作取消异常没有内部异常，则不用记录日志
				if(ex.InnerException == null)
					return;

				actionExecutedContext.Exception = ex.InnerException;
			}

			if(actionExecutedContext.Exception is Zongsoft.Security.Membership.AuthenticationException ||
			   actionExecutedContext.Exception is Zongsoft.Security.Membership.AuthorizationException)
			{
				actionExecutedContext.Response = this.GetExceptionResponse(actionExecutedContext.Exception, HttpStatusCode.Forbidden);

				//退出，不用记录日志
				return;
			}

			if(actionExecutedContext.Exception is NotSupportedException)
			{
				actionExecutedContext.Response = this.GetExceptionResponse(actionExecutedContext.Exception, HttpStatusCode.MethodNotAllowed);

				//退出，不用记录日志
				return;
			}

			if(actionExecutedContext.Exception is NotImplementedException)
			{
				actionExecutedContext.Response = this.GetExceptionResponse(actionExecutedContext.Exception, HttpStatusCode.NotImplemented);

				//退出，不用记录日志
				return;
			}

			var statusCode = HttpStatusCode.InternalServerError;

			if(actionExecutedContext.Exception is Zongsoft.Data.DataConflictException)
				statusCode = HttpStatusCode.Conflict;

			//生成返回的异常消息内容
			actionExecutedContext.Response = this.GetExceptionResponse(actionExecutedContext.Exception, statusCode);

			//默认将异常信息写入日志文件
			Zongsoft.Diagnostics.Logger.Error(actionExecutedContext.Exception, this.GetLoggingMessage(actionExecutedContext));
		}
		#endregion

		#region 私有方法
		private HttpResponseMessage GetExceptionResponse(Exception exception, HttpStatusCode status = HttpStatusCode.InternalServerError)
		{
			if(exception == null)
				return null;

			var message = exception.Message;

			if(exception.InnerException != null)
				message += @"\n" + exception.InnerException.Message;

			//将双引号替换成单引号；将回车符删除；将换行符替换成对应的转义标识
			if(message != null && message.Length > 0)
				message = message.Replace('"', '\'').Replace("\r", "").Replace("\n", "\\n");

			var response = new HttpResponseMessage(status)
			{
				Content = new StringContent($"{{\"type\":\"{exception.GetType().Name}\",\"message\":\"{message}\"}}", System.Text.Encoding.UTF8, "application/json")
			};

			return response;
		}

		private string GetLoggingMessage(HttpActionExecutedContext actionExecutedContext)
		{
			var text = new System.Text.StringBuilder();

			text.AppendLine("[" + actionExecutedContext.Request.Method.Method + "] " + actionExecutedContext.Request.RequestUri.OriginalString);
			text.AppendLine();

			if(actionExecutedContext.ActionContext.RequestContext.Principal != null)
				text.AppendLine(actionExecutedContext.ActionContext.RequestContext.Principal.ToString());

			text.AppendLine("{");
			foreach(var header in actionExecutedContext.Request.Headers)
			{
				text.AppendFormat("\t{0}: {1}" + Environment.NewLine, header.Key, this.GetHeaderValue(header.Value));
			}
			text.AppendLine("}");

			return text.ToString();
		}

		private string GetHeaderValue(IEnumerable<string> values)
		{
			if(values == null)
				return null;

			return string.Join("|", values);
		}
		#endregion
	}
}
