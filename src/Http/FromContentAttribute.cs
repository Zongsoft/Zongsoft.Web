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
		public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
		{
			return new FromContentBinding(parameter);
		}

		private class FromContentBinding : HttpParameterBinding
		{
			public FromContentBinding(HttpParameterDescriptor descriptor) : base(descriptor)
			{
			}

			public override async Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
			{
				if(actionContext.Request.Content.IsFormData())
				{
					var form = await actionContext.Request.Content.ReadAsFormDataAsync(cancellationToken);

					if(form != null && form.Count > 0)
					{
						var dictionary = new Dictionary<string, object>(form.Count, StringComparer.OrdinalIgnoreCase);

						foreach(var key in form.AllKeys)
						{
							dictionary.Add(key, form[key]);
						}

						this.SetParameters(actionContext, dictionary);
					}
				}
				else
				{
					if(string.Equals(actionContext.Request.Content.Headers.ContentType.MediaType, "text/json", StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(actionContext.Request.Content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
					{
						var text = await actionContext.Request.Content.ReadAsStringAsync();
						var dictionary = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<IDictionary<string, object>>(text);
						this.SetParameters(actionContext, dictionary);
					}
				}
			}

			private void SetParameters(HttpActionContext actionContext, IDictionary<string, object> dictionary)
			{
				if(dictionary == null || dictionary.Count < 1)
					return;

				var parameters = actionContext.ActionDescriptor.GetParameters();

				if(parameters == null || parameters.Count < 1)
					return;

				foreach(var entry in dictionary)
				{
					object value;
					var parameter = parameters.FirstOrDefault(p => string.Equals(p.ParameterName, entry.Key, StringComparison.OrdinalIgnoreCase));

					if(parameter != null && Zongsoft.Common.Convert.TryConvertValue(entry.Value, parameter.ParameterType, out value))
						actionContext.ActionArguments[parameter.ParameterName] = value;
				}
			}
		}
	}
}
