/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Runtime.Serialization.Json;

namespace Zongsoft.Web.Controls
{
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class Chart : CompositeDataBoundControl
	{
		#region 成员变量
		private ChartBinding _binding;
		private ChartPointCollection _points;
		private ChartLineCollection _lines;
		private ChartSerieCollection _series;
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue(ChartType.Pie)]
		public ChartType Type
		{
			get
			{
				return this.GetAttributeValue<ChartType>("Type", ChartType.Pie);
			}
			set
			{
				this.SetAttributeValue(() => this.Type, value);
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ChartBinding Binding
		{
			get
			{
				if(_binding == null)
					System.Threading.Interlocked.CompareExchange(ref _binding, new ChartBinding(), null);

				return _binding;
			}
		}

		public ChartPointCollection Points
		{
			get
			{
				if(_points == null)
					System.Threading.Interlocked.CompareExchange(ref _points, new ChartPointCollection(), null);

				return _points;
			}
		}

		public ChartSerieCollection Series
		{
			get
			{
				if(_series == null)
					System.Threading.Interlocked.CompareExchange(ref _series, new ChartSerieCollection(), null);

				return _series;
			}
		}

		public ChartLineCollection Lines
		{
			get
			{
				if(_lines == null)
					System.Threading.Interlocked.CompareExchange(ref _lines, new ChartLineCollection(), null);

				return _lines;
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		public string Title
		{
			get
			{
				return this.GetAttributeValue<string>("Title", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Title, value);
			}
		}

		public Unit Width
		{
			get
			{
				return this.GetAttributeValue<Unit>("Width", Unit.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Width, value);
			}
		}

		public Unit Height
		{
			get
			{
				return this.GetAttributeValue<Unit>("Height", Unit.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Height, value);
			}
		}
		#endregion

		#region 生成控件
		public override void RenderControl(HtmlTextWriter writer)
		{
			if(string.IsNullOrWhiteSpace(this.ID))
				throw new InvalidOperationException("The ID property is null or empty.");

			this.SetBinding();

			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID + "_data");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID + "_data");
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");

			using(MemoryStream stream = new MemoryStream())
			{
				DataContractJsonSerializer serializer = null;

				switch(this.Type)
				{
					case ChartType.Pie:
						serializer = new DataContractJsonSerializer(typeof(ChartPointCollection), new Type[] { typeof(ChartPoint) });
						serializer.WriteObject(stream, this.Points);
						break;
					case ChartType.Lines:
						serializer = new DataContractJsonSerializer(typeof(ChartLineCollection), new Type[] { typeof(ChartLine) });
						serializer.WriteObject(stream, this.Lines);
						break;
					case ChartType.Columns:
						serializer = new DataContractJsonSerializer(typeof(ChartLineCollection), new Type[] { typeof(ChartLine) });
						serializer.WriteObject(stream, this.Lines);
						break;
				}

				writer.AddAttribute(HtmlTextWriterAttribute.Value, Encoding.UTF8.GetString(stream.ToArray()));
			}

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			if(this.Type == ChartType.Columns || this.Type == ChartType.Lines)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID + "_series");
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID + "_series");
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");

				using(MemoryStream stream = new MemoryStream())
				{
					DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ChartSerieCollection), new Type[] { typeof(ChartSerie) });
					serializer.WriteObject(stream, this.Series);
					writer.AddAttribute(HtmlTextWriterAttribute.Value, Encoding.UTF8.GetString(stream.ToArray()));
				}

				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "chart-" + this.Type.ToString().ToLowerInvariant());
			writer.AddAttribute(HtmlTextWriterAttribute.Title, this.Title);

			if(this.Width.Value != 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());
			if(this.Height.Value != 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Height, this.Height.ToString());

			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag();
		}
		#endregion

		#region 私有方法
		private void SetBinding()
		{
			if(_binding == null || this.DataSource == null)
				return;

			IEnumerable dataSource = this.DataSource as IEnumerable;

			if(dataSource == null)
				return;

			if(this.Type == ChartType.Pie)
			{
				foreach(var dataItem in dataSource)
				{
					this.Points.Add(new ChartPoint()
					{
						Title = BindingUtility.FormatBindingValue(_binding.TitleMember, dataItem),
						Value = Zongsoft.Common.Convert.ConvertValue<decimal>(BindingUtility.GetBindingValue(_binding.ValueMember, dataItem)),
					});
				}
			}
			else
			{
				Dictionary<string, List<decimal>> lines = new Dictionary<string, List<decimal>>(StringComparer.OrdinalIgnoreCase);

				foreach(var dataItem in dataSource)
				{
					string key = BindingUtility.FormatBindingValue(_binding.TitleMember, dataItem);

					List<decimal> values;
					decimal v = Zongsoft.Common.Convert.ConvertValue<decimal>(BindingUtility.GetBindingValue(_binding.ValueMember, dataItem));

					if(lines.TryGetValue(key, out values))
					{
						values.Add(v);
					}
					else
					{
						values = new List<decimal>();
						values.Add(v);
						lines[key] = values;
					}

					if(!string.IsNullOrWhiteSpace(_binding.SerieMember))
					{
						string serie = BindingUtility.FormatBindingValue(_binding.SerieMember, dataItem);

						if(!this.Series.Contains(serie))
						{
							this.Series.Add(new ChartSerie()
							{
								Name = serie,
								Title = serie,
							});
						}
					}
				}

				foreach(var entry in lines)
				{
					this.Lines.Add(new ChartLine()
					{
						Name = entry.Key,
						Title = entry.Key,
						Values = entry.Value.ToArray(),
					});
				}
			}
		}
		#endregion

		#region 嵌套子类
		[Serializable]
		public class ChartBinding
		{
			public string SerieMember
			{
				get;
				set;
			}

			public string TitleMember
			{
				get;
				set;
			}

			public string ValueMember
			{
				get;
				set;
			}
		}

		public class ChartPointCollection : KeyedCollection<string, ChartPoint>
		{
			private static readonly ChartPoint[] Empty = new ChartPoint[0];

			internal ChartPointCollection() : base(StringComparer.OrdinalIgnoreCase)
			{
			}

			protected override string GetKeyForItem(ChartPoint item)
			{
				return item.Name;
			}

			public ChartPoint[] ToArray()
			{
				if(this.Count < 1)
					return Empty;

				ChartPoint[] result = new ChartPoint[this.Count];
				this.CopyTo(result, 0);

				return result;
			}
		}

		public class ChartLineCollection : KeyedCollection<string, ChartLine>
		{
			private static readonly ChartLine[] Empty = new ChartLine[0];

			internal ChartLineCollection() : base(StringComparer.OrdinalIgnoreCase)
			{
			}

			protected override string GetKeyForItem(ChartLine item)
			{
				return item.Name;
			}

			public ChartLine[] ToArray()
			{
				if(this.Count < 1)
					return Empty;

				ChartLine[] result = new ChartLine[this.Count];
				this.CopyTo(result, 0);

				return result;
			}
		}

		public class ChartSerieCollection : KeyedCollection<string, ChartSerie>
		{
			private static readonly ChartSerie[] Empty = new ChartSerie[0];

			internal ChartSerieCollection() : base(StringComparer.OrdinalIgnoreCase)
			{
			}

			protected override string GetKeyForItem(ChartSerie item)
			{
				return item.Name;
			}

			public ChartSerie[] ToArray()
			{
				if(this.Count < 1)
					return Empty;

				ChartSerie[] result = new ChartSerie[this.Count];
				this.CopyTo(result, 0);

				return result;
			}
		}
		#endregion
	}
}
