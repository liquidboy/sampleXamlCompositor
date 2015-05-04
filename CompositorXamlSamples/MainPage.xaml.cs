using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace CompositorXamlSamples
{
    public sealed partial class MainPage : Page
    {

        ContainerVisual _bottomVisual;
        Compositor _compositor;
        CompositionImage _largeImage;
        
        Microsoft.Graphics.Canvas.Effects.SaturationEffect _saturationDefinition;
        CompositionEffect _saturationEffect;
        EffectVisual _saturationVisual;


        public MainPage()
        {
            this.InitializeComponent();

            App.Current.Suspending += (o,e) => {
                try {
                    CleanUp();
                }
                catch (Exception ex) {
                    
                }
                
            };
        }

        
        private void CleanUp() {
            
            if (_bottomVisual != null) {
                _bottomVisual.Children.RemoveAll();

                _bottomVisual.Dispose();
                _compositor.Dispose();
                _largeImage.Dispose();
                _saturationDefinition.Dispose();
                _saturationEffect.Dispose();
                _saturationVisual.Dispose();
            }
        }

        private void layoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (_bottomVisual == null)
            {
                _bottomVisual = (ContainerVisual)ElementCompositionPreview.GetContainerVisual(bottomSurface);
                _compositor = _bottomVisual.Compositor;
            }


            InitCompositionBits();

        }

        private async void InitCompositionBits() {

            if (_largeImage != null) _largeImage.Dispose();

            //load image content
            _largeImage = _compositor.DefaultGraphicsDevice.CreateImageFromUri(new Uri("ms-appx:///Assets/Flower2.jpg"));

            //wait for image to finish loading (which gives us its properties like size)
            await _largeImage.CompleteLoadAsync();

            
            //setup definition of effect
            _saturationDefinition = new Microsoft.Graphics.Canvas.Effects.SaturationEffect();
            _saturationDefinition.Saturation = 1f;
            _saturationDefinition.Name = "sat";
            _saturationDefinition.Source = new CompositionEffectSourceParameter("Image");

            //effect factory to generate our effects (based on the definiton above)
            CompositionEffectFactory cef = _compositor.CreateEffectFactory(_saturationDefinition);

            //create our effect and set any settings on it
            _saturationEffect = cef.CreateEffect();
            _saturationEffect.SetSourceParameter("Image", _largeImage);

            //create a compositor visual that we apply our effect on
            _saturationVisual = _compositor.CreateEffectVisual();
            _saturationVisual.Effect = _saturationEffect;
            _saturationVisual.Size = new System.Numerics.Vector2((float)_largeImage.Size.Width, (float)_largeImage.Size.Height);
            _saturationVisual.Offset = new System.Numerics.Vector3((float)75, (float)75, 0);
            _saturationVisual.Scale = new System.Numerics.Vector3(0.4f, 0.4f, 0);

            //add the visual into the tree for rendering
            _bottomVisual.Children.InsertAtBottom(_saturationVisual);
            
        }
        
        private void sliderSaturation_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_saturationDefinition == null) return;

            //update saturation
            _saturationDefinition.Saturation = (float)e.NewValue;

            //recreate effect which has updated saturation
            CompositionEffectFactory cef = _compositor.CreateEffectFactory(_saturationDefinition);
            _saturationEffect = cef.CreateEffect();
            _saturationEffect.SetSourceParameter("Image", _largeImage);

            //update existing visual with newly recreated effect
            _saturationVisual.Effect = _saturationEffect;
        }
    }
}
