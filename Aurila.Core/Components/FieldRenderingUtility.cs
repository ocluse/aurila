using Aurila.Contracts.Components;
using Ocluse.LiquidSnow.Extensions;

namespace Aurila.Components;

internal class FieldRenderingUtility
{
    public static void BuildFieldHeader(RenderTreeBuilder builder, IFieldComponent field, FieldHeaderStyle headerStyle)
    {
        if (field.Header != null)
        {
            builder.OpenElement(1, "div");
            {
                builder.AddAttribute(2, "class", $"au-field__header {headerStyle.ToString().ToLowerInvariant()}");
                builder.AddContent(3, field.Header);
            }
            builder.CloseElement();
        }
    }

    public static void BuildField(RenderTreeBuilder builder, IFieldComponent field, IAppearanceProvider appearanceProvider)
    {
        var headerStyle = appearanceProvider.HeaderStyle;

        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, field.GetAppliedAttributes());

            //Static header:
            if (headerStyle == FieldHeaderStyle.Static)
            {
                builder.OpenRegion(3);
                {
                    BuildFieldHeader(builder, field, headerStyle);
                }
                builder.CloseRegion();
            }

            //The input content
            builder.OpenElement(4, "div");
            {
                builder.AddAttribute(5, "class", "au-field__content");

                if (field.Prefix != null)
                {
                    builder.OpenElement(6, "div");
                    {
                        builder.AddAttribute(7, "class", "au-field__content__prefix");
                        builder.AddContent(8, field.Prefix);
                    }
                    builder.CloseElement();
                }

                builder.OpenRegion(9);
                {
                    field.BuildInput(builder);
                }
                builder.CloseRegion();

                //Floating header
                if (headerStyle == FieldHeaderStyle.Floating)
                {
                    builder.OpenRegion(10);
                    {
                        BuildFieldHeader(builder, field, headerStyle);
                    }
                    builder.CloseElement();
                }

                if (field.Suffix != null)
                {
                    builder.OpenElement(11, "div");
                    {
                        builder.AddAttribute(12, "class", "au-field__content__suffix");
                        builder.AddContent(13, field.Suffix);
                    }
                    builder.CloseElement();
                }
            }
            builder.CloseElement();

            var validation = field.Validation;

            //TODO: Its possible that the validation message is empty, but the validation is not null. This API therefore cuts out this case.
            if (validation != null && validation.Message.IsNotEmpty() == true)
            {
                //Validation message
                if (field.ValidationLabel != null)
                {
                    builder.OpenElement(14, "div");
                    {
                        builder.AddAttribute(15, "class", "au-field__validation-label");
                        builder.AddContent(16, field.ValidationLabel(validation));
                    }
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(17, "div");
                    {
                        builder.AddAttribute(18, "class", "au-field__validation-label");
                        builder.AddAttribute(19, "role", "alert");
                        if (validation?.Message.IsNotEmpty() == true)
                        {
                            builder.AddContent(20, validation.Message);
                        }
                    }
                    builder.CloseElement();
                }
            }

            if (field is IAuxiliaryContentFieldComponent auxiliaryContentField)
            {
                builder.OpenRegion(21);
                {
                    auxiliaryContentField.BuildAuxiliaryContent(builder);
                }
                builder.CloseRegion();
            }
        }
        builder.CloseElement();
    }
}
