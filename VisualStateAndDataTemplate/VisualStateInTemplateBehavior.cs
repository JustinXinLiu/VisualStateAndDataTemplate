using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace VisualStateAndDataTemplate
{
    public class VisualStateInTemplateBehavior : Behavior<UserControl>
    {
        #region Fields

        private readonly TaskCompletionSource<bool> _userControlLoaded = new TaskCompletionSource<bool>();

        #endregion

        #region Properties

        public string ParentNarrowStateName { get; set; } = "NarrowState";
        public string ParentNormalStateName { get; set; } = "NormalState";
        public string ParentWideStateName { get; set; } = "WideState";

        public VisualState NarrowState
        {
            get => (VisualState)GetValue(NarrowStateProperty);
            set => SetValue(NarrowStateProperty, value);
        }
        public static readonly DependencyProperty NarrowStateProperty = DependencyProperty.Register(
            "NarrowState", typeof(VisualState), typeof(VisualStateInTemplateBehavior), new PropertyMetadata(null));

        public VisualState NormalState
        {
            get => (VisualState)GetValue(NormalStateProperty);
            set => SetValue(NormalStateProperty, value);
        }
        public static readonly DependencyProperty NormalStateProperty = DependencyProperty.Register(
            "NormalState", typeof(VisualState), typeof(VisualStateInTemplateBehavior), new PropertyMetadata(null));

        public VisualState WideState
        {
            get => (VisualState)GetValue(WideStateProperty);
            set => SetValue(WideStateProperty, value);
        }
        public static readonly DependencyProperty WideStateProperty = DependencyProperty.Register(
            "WideState", typeof(VisualState), typeof(VisualStateInTemplateBehavior), new PropertyMetadata(null));

        public VisualStateGroup ParentVisualStateGroup
        {
            get => (VisualStateGroup)GetValue(ParentVisualStateGroupProperty);
            set => SetValue(ParentVisualStateGroupProperty, value);
        }
        public static readonly DependencyProperty ParentVisualStateGroupProperty = DependencyProperty.Register(
            "ParentVisualStateGroup", typeof(VisualStateGroup), typeof(VisualStateInTemplateBehavior),
            new PropertyMetadata(null, OnParentStateGroupChanged));

        #endregion

        #region Overrides

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
            SetupNewVisualStateGroup();
            base.OnAttached();

            async void SetupNewVisualStateGroup()
            {
                var newGroup = new VisualStateGroup();
                newGroup.States.Add(NarrowState);
                newGroup.States.Add(NormalState);
                newGroup.States.Add(WideState);

                await _userControlLoaded.Task;
                var groups = VisualStateManager.GetVisualStateGroups((FrameworkElement)AssociatedObject.Content);
                groups.Add(newGroup);
            }
        }

        protected override void OnDetaching()
        {
            ParentVisualStateGroup.CurrentStateChanged -= OnCurrentStateChanged;
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;

            base.OnDetaching();
        }

        #endregion

        #region Event Handlers

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e) =>
            _userControlLoaded.SetResult(true);

        private static async void OnParentStateGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (VisualStateInTemplateBehavior)d;

            if (e.OldValue != null)
            {
                var oldParentGroup = (VisualStateGroup)e.OldValue;
                oldParentGroup.CurrentStateChanged -= self.OnCurrentStateChanged;
            }

            if (e.NewValue != null)
            {
                var newParentGroup = (VisualStateGroup)e.NewValue;
                newParentGroup.CurrentStateChanged += self.OnCurrentStateChanged;

                await self._userControlLoaded.Task;
                self.GoToCorrenspondingState(newParentGroup.CurrentState?.Name);
            }
        }

        private void OnCurrentStateChanged(object sender, VisualStateChangedEventArgs e) =>
            GoToCorrenspondingState(e.NewState.Name);

        #endregion

        #region Methods

        private void GoToCorrenspondingState(string stateName)
        {
            if (stateName == ParentNarrowStateName)
            {
                VisualStateManager.GoToState(AssociatedObject, NarrowState.Name, false);
            }
            else if (stateName == ParentWideStateName)
            {
                VisualStateManager.GoToState(AssociatedObject, WideState.Name, false);
            }
            else
            {
                VisualStateManager.GoToState(AssociatedObject, NormalState.Name, false);
            }
        }

        #endregion
    }
}
