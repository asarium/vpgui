using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace VPGUI.Behaviors
{
    class TextHighlightBehavior : Behavior<TextBlock>
    {
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("HightlightText", typeof (string), typeof(TextHighlightBehavior),
            new PropertyMetadata(default(string), HightlightText_Changed));

        public string HightlightText
        {
            get { return (string) GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        private static void HightlightText_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var behavior = dependencyObject as TextHighlightBehavior;

            if (behavior != null)
            {
                behavior.UpdateHightlight();
            }
        }

        private static readonly DependencyPropertyDescriptor TextDescriptor = 
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

        private bool _updatingHighlight = false;
        private void UpdateHightlight()
        {
            if (this._updatingHighlight)
            {
                // Avoid infinite recursion
                return;
            }

            this._updatingHighlight = true;

            try
            {
                if (this.AssociatedObject == null)
                {
                    return;
                }
                var text = this.AssociatedObject.Text;

                this.AssociatedObject.Inlines.Clear();

                if (this.HightlightText.Length <= 0)
                {
                    this.AssociatedObject.Inlines.Add(this.AssociatedObject.Text);
                    return;
                }

                var highlight = this.HightlightText;

                var index = 0;
                var lastEnd = index;

                while ((index = text.IndexOf(highlight, lastEnd, StringComparison.InvariantCultureIgnoreCase)) >= 0)
                {
                    // Add not highlighted text
                    var length = index - lastEnd;

                    if (length > 0)
                    {
                        this.AssociatedObject.Inlines.Add(text.Substring(lastEnd, length));
                    }

                    // Add highlighted Text
                    var r = new Run(text.Substring(index, highlight.Length)) {
                        FontWeight = FontWeights.Bold
                    };

                    this.AssociatedObject.Inlines.Add(r);

                    r.SetResourceReference(TextElement.ForegroundProperty, "AccentColorBrush");
                    
                    lastEnd = index + highlight.Length;
                }

                if (lastEnd + 1 < text.Length)
                {
                    this.AssociatedObject.Inlines.Add(text.Substring(lastEnd));
                }
            }
            finally
            {
                this._updatingHighlight = false;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            TextDescriptor.AddValueChanged(AssociatedObject, TextBlock_TextChanged);

            UpdateHightlight();
        }

        private void TextBlock_TextChanged(object sender, EventArgs eventArgs)
        {
            UpdateHightlight();
        }

        protected override void OnDetaching()
        {
            TextDescriptor.RemoveValueChanged(AssociatedObject, TextBlock_TextChanged);

            base.OnDetaching();
        }
    }
}
