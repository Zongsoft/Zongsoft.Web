# Zongsoft.Web

![license](https://img.shields.io/github/license/Zongsoft/Zongsoft.Web) ![download](https://img.shields.io/nuget/dt/Zongsoft.Web) ![version](https://img.shields.io/github/v/release/Zongsoft/Zongsoft.Web?include_prereleases) ![github stars](https://img.shields.io/github/stars/Zongsoft/Zongsoft.Web?style=social)

README: [English](https://github.com/Zongsoft/Zongsoft.Web/blob/master/README.md) | [简体中文](https://github.com/Zongsoft/Zongsoft.Web/blob/master/README-zh_CN.md)

-----

提供了 **API** 的控制器基类、HTTP上传处理的工具类、身份验证和授权控制的过滤器等，还包括一套自定义的 Web 控件集，基于前后端分离的Web开发模式，今后将不再支持 Web 控件。


## 控件设计

所有控件均从 `DataBoundControl` 类继承，该基类指定 `DataBoundControlBuilder` 来解析页面中的控件元素。

### 控件属性

控件的属性可以通过 `PropertyMetadataAttribute` 标记类来指定相应的生成规则，该类的定义如下：

``` C#
[AttributeUsage(AttributeTargets.Property)]
public class PropertyMetadataAttribute : Attribute
{
	// 表示当前控件属性对应生成的HTML特性(attribute)名称，默认值为空(null)，表示取对应属性的名称。
	public string AttributeName {get; set;}

	// 表示当前控件属性是否要生成对应的HTML特性(attribute)，默认值为真(True)
	public bool Renderable {get; set;}

	// 获取或设置控件属性是否支持绑定表达式，默认为真(True)。
	public bool Bindable {get; set;}

	// 获取或设置属性生成器的实例类型或获取该实例的静态属性路径文本。
	public object PropertyRender {get; set;}
}
```

控件属性均使用从 `DataBoundControl` 基类中的 `GetPropertyValue(...)` 方法获取属性值和 `SetPropertyValue(...)` 方法来设置属性值。

如果控件属性未声明 `PropertyMetadataAttribute`，则表示生成同名的 `HTML特性(attribute)`，也可以通过指定 `PropertyMetadataAttribute.AttributeName` 属性来显式设定要生成的 `HTML特性(attribute)` 的名称；可通过 `PropertyMetadataAttribute.Renderable` 属性来指示是否需要生成对应的 `HTML特性(attribute)`。

控件属性通过 `BindableAttribute.Bindable` 或者 `PropertyMetadataAttribute.Bindable` 来表示是否支持绑定表达式。

关于控件属性的一般编写方法，请参考 InputBox 控件的代码，大致如下：

``` C#
public class InputBox : DataBoundControl
{
	[Bindable(true)]
	[DefaultValue("")]
	public string Name
	{
		get
		{
			return this.GetPropertyValue(() => this.Name);
		}
		set
		{
			this.SetPropertyValue(() => this.Name, value);
		}
	}

	[Bindable(true)]
	[DefaultValue(true)]
	[PropertyMetadata("disabled", PropertyRender = "BooleanPropertyRender.False")]
	public bool Enabled
	{
		get
		{
			return this.GetPropertyValue(() => this.Enabled);
		}
		set
		{
			this.SetPropertyValue(() => this.Enabled, value);
		}
	}

	[DefaultValue(InputBoxType.Text)]
	[PropertyMetadata("type")]
	public virtual InputBoxType InputType
	{
		get
		{
			return this.GetPropertyValue(() => this.InputType);
		}
		set
		{
			this.SetPropertyValue(() => this.InputType, value);

			switch(value)
			{
				case InputBoxType.Button:
					this.CssClass = "ui-button";
					break;
				case InputBoxType.Reset:
					this.CssClass = "ui-button reset";
					break;
				case InputBoxType.Submit:
					this.CssClass = "ui-button submit";
					break;
				case InputBoxType.File:
					this.CssClass = "ui-file";
					break;
				case InputBoxType.Image:
					this.CssClass = "ui-image";
					break;
				case InputBoxType.CheckBox:
					this.CssClass = "ui-checkbox";
					break;
				case InputBoxType.Radio:
					this.CssClass = "ui-radio";
					break;
				case InputBoxType.Text:
					this.CssClass = "ui-input";
					break;
				case InputBoxType.Password:
					this.CssClass = "ui-input password";
					break;
				default:
					this.CssClass = "ui-input " + value.ToString().ToLowerInvariant();
					break;
			}
		}
	}

	[Bindable(true)]
	[DefaultValue("")]
	[PropertyMetadata(false)]
	public string Label
	{
		get
		{
			return this.GetPropertyValue(() => this.Label);
		}
		set
		{
			this.SetPropertyValue(() => this.Label, value);
		}
	}

	[Bindable(true)]
	[DefaultValue("")]
	public string Value
	{
		get
		{
			return this.GetPropertyValue(() => this.Value);
		}
		set
		{
			this.SetPropertyValue(() => this.Value, value);
		}
	}
}
```


### 样式类名

通常每个控件会有自己的默认样式类名(即HTML元素的CSS样式名)，但是可以显式设置控件的 `CSSClass` 属性来覆盖或追加(以冒号打头)样式类名，用法大致如下：

``` HTML
<!-- 覆盖默认的控件属性的样式类名 -->
<zs:InputBox InputType="Password" CssClass="MyCssClass" runat="server" />

<!-- 在默认的控件属性的样式类名后追加指定的样式类名 -->
<zs:InputBox InputType="Password" CssClass=":MyFirstCssClass MySecondCssClass" runat="server" />
```
