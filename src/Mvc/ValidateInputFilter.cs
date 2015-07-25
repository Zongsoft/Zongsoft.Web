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
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Zongsoft.Web.Mvc
{
	public class ValidateInputFilter : IActionFilter
	{
		#region 输入检测
		public void OnActionExecuting(ActionExecutingContext context)
		{
			if(context.ActionParameters == null || context.ActionParameters.Count < 1)
				return;

			var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach(var entry in context.ActionParameters)
			{
				if(entry.Value == null)
					continue;

				if(entry.Value.GetType() == typeof(string))
				{
					dictionary[entry.Key] = ValidateInputUtility.Detoxify((string)entry.Value);
				}
				else if(entry.Value.GetType().IsClass)
				{
					var properties = TypeDescriptor.GetProperties(entry.Value);

					foreach(PropertyDescriptor property in properties)
					{
						if(property.PropertyType == typeof(string))
							property.SetValue(entry.Value, ValidateInputUtility.Detoxify((string)property.GetValue(entry.Value)));
					}
				}
			}

			if(dictionary != null && dictionary.Count > 0)
			{
				foreach(var entry in dictionary)
				{
					context.ActionParameters[entry.Key] = entry.Value;
				}
			}
		}
		#endregion

		#region 显式实现
		void IActionFilter.OnActionExecuted(ActionExecutedContext context)
		{
		}
		#endregion
	}
}
