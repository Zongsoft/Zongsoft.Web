/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Zongsoft.Web.Http
{
	public static class HttpResponseExceptionUtility
	{
		public static HttpResponseException BadRequest(string message = null)
		{
			if(string.IsNullOrWhiteSpace(message))
				return new HttpResponseException(HttpStatusCode.BadRequest);

			var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
				Content = new StringContent(@"{""message"":""" + EscapeQuote(message) + @"""}", System.Text.Encoding.UTF8, "application/json"),
			};

			return new HttpResponseException(response);
		}

		/// <summary>
		/// 生成因为模型验证失败导致的无效请求的响应消息。
		/// </summary>
		/// <param name="states">当前请求的模型状态字典。</param>
		/// <returns>返回的无效请求响应消息。</returns>
		/// <remarks>
		///		<para>返回的响应消息内容为JSON，其格式如下所示：</para>
		///		<code><![CDATA[
		///	[
		///		{
		///			EmployeeNo:
		///			[
		///				{type:"The type of exception #1", message:"exception message"},
		///				{type:"The type of exception #2", message:"exception message"}
		///			]
		///		},
		///		{
		///			Identity:
		///			[
		///				{type:"The type of exception", message:"exception message"}
		///			]
		///		}
		///	]
		///		]]>
		///		</code>
		/// </remarks>
		public static HttpResponseException BadRequest(ModelStateDictionary states)
		{
			if(states == null || states.Count == 0)
				return new HttpResponseException(HttpStatusCode.BadRequest);

			var index = 0;
			var text = new System.Text.StringBuilder("[");

			foreach(var state in states)
			{
				if(text.Length > 1)
					text.AppendLine(",");

				text.AppendFormat("{{\"{0}\":[", state.Key);

				for(int j = 0; j < state.Value.Errors.Count; j++)
				{
					var error = state.Value.Errors[j];

					if(error.Exception == null)
					{
						text.AppendFormat("{{\"message:\":\"{0}\"", EscapeQuote(error.ErrorMessage));
					}
					else
					{
						var exception = GetInnerException(error.Exception);
						text.AppendFormat("{{\"type\":\"{0}\",\"message\":\"{1}\"}}", exception.GetType().Name, EscapeQuote(exception.Message));
					}

					if(j < state.Value.Errors.Count - 1)
						text.Append(",");
				}

				text.Append("]}");

				if(index++ < states.Count - 1)
					text.AppendLine(",");
			}

			text.Append("]");

			return new HttpResponseException(
				new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
				{
					Content = new StringContent(text.ToString(), System.Text.Encoding.UTF8, "application/json")
				});
		}

		#region 私有方法
		private static string EscapeQuote(string text)
		{
			//如果指定的字符串中没有包含双引号则表示该字符串不用转义处理
			if(string.IsNullOrEmpty(text) || text.IndexOf('"') < 0)
				return text;

			var escape = false;
			var buffer = new System.Text.StringBuilder(text.Length + (int)Math.Max(10, text.Length * 0.2));

			foreach(var chr in text)
			{
				//如果当前字符为双引号，并且当前不是转义状态，则往缓存中添加一个转义字符（即反斜杠）
				if(chr == '"' && !escape)
					buffer.Append('\\');

				//判断当前字符是否为转义字符
				escape = chr == '\\' && !escape;

				//将当前字符加入缓存
				buffer.Append(chr);
			}

			return buffer.ToString();
		}

		private static Exception GetInnerException(Exception exception)
		{
			if(exception == null)
				return null;

			if(exception.InnerException == null)
				return exception;

			return GetInnerException(exception.InnerException);
		}
		#endregion
	}
}
