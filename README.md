Zongsoft.Web
============

关于Web程序常用功能的类库，提供了一套基于数据绑定表达式的控件和主题库。


## 控件设计

所有控件均从 `DataBoundControl` 类继承。控件的属性可以通过 `PropertyMetadataAttribute` 标记类来指定相应的生成规则，该类的定义如下：

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
如果控件属性未声明 `PropertyMetadataAttribute`，则表示生成同名的 HTML 特性(attribute)，如果控件属性未声明 `DefaultValueAttribute` 并且该控件的使用者也未显式对该属性进行设置的话，则不会为其生成对应的 HTML 特性(attribute)。

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
