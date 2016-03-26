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
		#region 成员字段
		private System.Text.StringBuilder _script;
		#endregion

		#region 内部属性
		protected System.Text.StringBuilder Script
		{
			get
			{
				if(_script == null)
					System.Threading.Interlocked.CompareExchange(ref _script, new System.Text.StringBuilder(), null);

				return _script;
			}
		}
		#endregion

		#region 生成方法
		protected override void Render(HtmlTextWriter writer)
		{
			if(this.Page == null || this.Page.ModelState.IsValid)
			{
				this.RenderScript(writer, "var validation_info = null;");

				return;
			}

			int fieldCount = 0;

			foreach(var key in this.Page.ModelState.Keys)
			{
				var state = this.Page.ModelState[key];

				if(state.Errors.Count < 1)
					continue;

				if(string.IsNullOrWhiteSpace(key))
				{
					this.Script.AppendFormat("summary : [");

					for(int i = 0; i < state.Errors.Count; i++)
					{
						if(i > 0)
							this.Script.Append(", ");

						this.Script.AppendFormat("'{0}'", state.Errors[i].ErrorMessage.Replace('\'', '"'));
					}

					this.Script.Append("]");
				}
				else
				{
					if(fieldCount++ == 0)
					{
						if(this.Script.Length > 0)
							this.Script.AppendLine(",");

						this.Script.AppendLine("fields : [");
					}

					if(fieldCount > 1)
						this.Script.AppendLine(",");

					this.Script.AppendLine("{");
					this.Script.AppendLine("\tidentifier : '" + key + "',");
					this.Script.Append("\tmessages : [");

					for(int i = 0; i < state.Errors.Count; i++)
					{
						if(i > 0)
							this.Script.Append(", ");

						this.Script.AppendFormat("'{0}'", state.Errors[i].ErrorMessage.Replace('\'', '"'));
					}

					this.Script.AppendLine("]");
					this.Script.Append("}");
				}
			}

			if(fieldCount > 0)
				this.Script.AppendLine("]");

			if(_script != null && _script.Length > 0)
			{
				//this.Page.ClientScript.RegisterStartupScript(this.GetType(),
				//	string.IsNullOrWhiteSpace(this.ID) ? this.GetType().Name : this.ID,
				//	"var validation_info = {" + Environment.NewLine + _script.ToString() + "}", true);

				this.RenderScript(writer, "var validation_info = {" + Environment.NewLine + _script.ToString() + "};");
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
		#endregion
	}
}
