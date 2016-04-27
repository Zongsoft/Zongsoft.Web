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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
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
using System.Web;
using System.Web.UI;
using System.Web.ModelBinding;

namespace Zongsoft.Web.Controls
{
	public class ValidationSummary : DataBoundControl
	{
		#region 生成方法
		protected override void Render(HtmlTextWriter writer)
		{
			if(this.Page == null || this.Page.ModelState.IsValid)
			{
				this.RenderScript(writer, "var validation_info = null;");
				return;
			}

			var count = 0;
			var summary = new System.Text.StringBuilder();
			var fields = new System.Text.StringBuilder();

			foreach(var key in this.Page.ModelState.Keys)
			{
				var state = this.Page.ModelState[key];

				if(state.Errors.Count < 1)
					continue;

				if(string.IsNullOrWhiteSpace(key))
				{
					count = 0;

					foreach(var error in state.Errors)
					{
						if(!string.IsNullOrWhiteSpace(error.ErrorMessage))
						{
							if(count++ > 0)
								summary.AppendLine(", ");

							summary.AppendFormat("'{0}'", RepairMessage(error.ErrorMessage.Replace('\'', '"')));
						}
					}
				}
				else
				{
					if(fields.Length > 0)
						fields.AppendLine(", ");

					fields.AppendLine("{");
					fields.AppendLine("\tidentifier: '" + key + "',");
					fields.Append("\tmessages: [");

					count = 0;

					foreach(var error in state.Errors)
					{
						if(!string.IsNullOrWhiteSpace(error.ErrorMessage))
						{
							if(count++ > 0)
								fields.Append(", ");

							fields.AppendFormat("'{0}'", RepairMessage(error.ErrorMessage.Replace('\'', '"')));
						}
					}

					fields.AppendLine("]");
					fields.Append("}");
				}
			}

			if(fields.Length > 0 || summary.Length > 0)
			{
				//this.Page.ClientScript.RegisterStartupScript(this.GetType(),
				//	string.IsNullOrWhiteSpace(this.ID) ? this.GetType().Name : this.ID,
				//	"var validation_info = {" + Environment.NewLine + _script.ToString() + "}", true);

				this.RenderScript(writer, "var validation_info = {" + Environment.NewLine +
					"fields: [" + Environment.NewLine + fields.ToString() + "]," + Environment.NewLine +
					"summary: [" + summary.ToString() + "]" + Environment.NewLine +
					"};");
			}
		}
		#endregion

		#region 私有方法
		private void RenderScript(HtmlTextWriter writer, string script)
		{
			if(writer == null || string.IsNullOrEmpty(script))
				return;

			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
			writer.RenderBeginTag(HtmlTextWriterTag.Script);
			writer.Write(script);
			writer.RenderEndTag();
		}

		private string RepairMessage(string message)
		{
			if(string.IsNullOrWhiteSpace(message))
				return string.Empty;

			return message.Replace('\r', ' ').Replace('\n', ' ');
		}
		#endregion
	}
}
