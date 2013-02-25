using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace VPGUI.Behaviors
{
    class EditTextBoxBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditTextBoxBehavior),
            new PropertyMetadata(default(bool), IsEditing_Changed));

        public bool IsEditing
        {
            get
            {
                return (bool) GetValue(IsEditingProperty);
            }
            set
            {
                SetValue(IsEditingProperty, value);
            }
        }

        private static void IsEditing_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var behavior = dependencyObject as EditTextBoxBehavior;

            if (behavior != null)
            {
                var editable = (IEditableObject) behavior.AssociatedObject.DataContext;

                if (behavior.IsEditing)
                {
                    editable.BeginEdit();
                }
                else
                {
                    editable.EndEdit();
                }
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            var editable = AssociatedObject.DataContext as IEditableObject;

            if (editable == null)
            {
                throw new ArgumentException("DataContext of the TextBox is not editable!");
            }
            
            AssociatedObject.IsVisibleChanged += TextBox_IsVisibleChanged;
            AssociatedObject.KeyUp += TextBox_OnKeyUp;
            AssociatedObject.LostFocus += TextBox_FocusLost;
        }

        private void TextBox_FocusLost(object sender, RoutedEventArgs e)
        {
            var editable = (IEditableObject) AssociatedObject.DataContext;

            editable.EndEdit();
        }

        private void TextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                var editable = (IEditableObject) AssociatedObject.DataContext;

                if (e.Key == Key.Escape)
                {
                    editable.CancelEdit();
                }
                else
                {
                    editable.EndEdit();
                }
            }
        }

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var editable = (IEditableObject) AssociatedObject.DataContext;

            if ((bool) e.NewValue)
            {
                AssociatedObject.Focus();
                AssociatedObject.SelectAll();
            }
            else
            {
                editable.EndEdit();
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.IsVisibleChanged -= TextBox_IsVisibleChanged;
            AssociatedObject.KeyUp -= TextBox_OnKeyUp;
            AssociatedObject.LostFocus -= TextBox_FocusLost;

            base.OnDetaching();
        }
    }
}
