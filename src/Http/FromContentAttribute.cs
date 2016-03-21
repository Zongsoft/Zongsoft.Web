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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Metadata;
using System.Web.Http.Controllers;

namespace Zongsoft.Web.Http
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class FromContentAttribute : ParameterBindingAttribute
	{
		#region 重写方法
		public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
		{
			return new FromContentBinding(parameter);
		}
		#endregion

		private class FromContentBinding : HttpParameterBinding
		{
			#region 私有常量
			private static readonly string CacheKey = "__" + typeof(FromContentBinding).Name + "__";
			#endregion

			#region 构造函数
			public FromContentBinding(HttpParameterDescriptor descriptor) : base(descriptor)
			{
			}
			#endregion

			#region 重写方法
			public override async Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
			{
				//从缓存中获取内容字典
				IDictionary<string, object> dictionary = this.GetCachedContent(actionContext);

				if(dictionary == null)
				{
					if(actionContext.Request.Content.IsFormData())
					{
						var form = await actionContext.Request.Content.ReadAsFormDataAsync();

						if(form != null && form.Count > 0)
						{
							dictionary = new Dictionary<string, object>(form.Count, StringComparer.OrdinalIgnoreCase);

							foreach(var key in form.AllKeys)
							{
								dictionary[key] = form[key];
							}
						}
					}
					else if(actionContext.Request.Content.IsMimeMultipartContent())
					{
						var multipart = await actionContext.Request.Content.ReadAsMultipartAsync();

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
					else if(string.Equals(actionContext.Request.Content.Headers.ContentType.MediaType, "text/json", StringComparison.OrdinalIgnoreCase) ||
							string.Equals(actionContext.Request.Content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
					{
						var text = await actionContext.Request.Content.ReadAsStringAsync();

						if(!string.IsNullOrWhiteSpace(text))
						{
							var result = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<Dictionary<string, object>>(text);

							//将区分大小写字典键转换为不区分大小写的字典
							dictionary = new Dictionary<string, object>(result, StringComparer.OrdinalIgnoreCase);
						}
					}

					//将解析后的内容字典保存到当前操作的扩展属性中缓存起来
					actionContext.ActionDescriptor.Properties[CacheKey] = dictionary;
				}

				object value;

				if(this.Descriptor.ParameterType is IDictionary || Zongsoft.Common.TypeExtension.IsAssignableFrom(typeof(IDictionary<string, object>), this.Descriptor.ParameterType))
					this.SetValue(actionContext, dictionary);
				else if(dictionary != null && dictionary.TryGetValue(this.Descriptor.ParameterName, out value))
					this.SetValue(actionContext, Zongsoft.Common.Convert.ConvertValue(value, this.Descriptor.ParameterType));
			}
			#endregion

			#region 私有方法
			private IDictionary<string, object> GetCachedContent(HttpActionContext actionContext)
			{
				object cachedContent;

				if(actionContext.ActionDescriptor.Properties.TryGetValue(CacheKey, out cachedContent) && cachedContent is IDictionary<string, object>)
					return (IDictionary<string, object>)cachedContent;

				return null;
			}
			#endregion
		}
	}
}
