using System;
using Foundation;
using UIKit;
using ImageSearch.ViewModel;
using SDWebImage;
using System.Linq;
using Acr.UserDialogs;

namespace ImageSearch.iOS
{
    public partial class ViewController : UIViewController, IUICollectionViewDataSource, IUICollectionViewDelegate
    {
        ImageSearchViewModel viewModel;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            viewModel = new ImageSearchViewModel();

            CollectionViewImages.WeakDataSource = this;
            CollectionViewImages.AllowsSelection = true;

            CollectionViewImages.Delegate = this;

            ButtonSearch.TouchUpInside += async (sender, e) =>
            {
                ButtonSearch.Enabled = false;
                ActivityIsLoading.StartAnimating();

                await viewModel.SearchForImagesAsync(TextFieldQuery.Text);
                CollectionViewImages.ReloadData();

                ButtonSearch.Enabled = true;
                ActivityIsLoading.StopAnimating();
            };

            var cameraButton = new UIBarButtonItem(UIBarButtonSystemItem.Camera, 
            async (sender, e) =>
            {
                ActivityIsLoading.StartAnimating();
                await viewModel.TakePhotAsync();
                ActivityIsLoading.StopAnimating();
            });

            var pickButton = new UIBarButtonItem(UIBarButtonSystemItem.Organize,
            async (sender, e) =>
            {
                ActivityIsLoading.StartAnimating();
                await viewModel.TakePhotAsync(false);
                ActivityIsLoading.StopAnimating();
            });

            this.NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { cameraButton, pickButton };
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        public async void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            ActivityIsLoading.StartAnimating();

            string description = await viewModel.GetImageDescription(viewModel.Images[indexPath.Row].ImageLink);
            UIAlertView alert = new UIAlertView("Image Analysis",
                                                description, null, "OK", null);
            alert.Show();

            ActivityIsLoading.StopAnimating();
        }

        public nint GetItemsCount(UICollectionView collectionView, nint section) =>
            viewModel.Images.Count;

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell("imagecell", indexPath) as ImageCell;

            var item = viewModel.Images[indexPath.Row];

            cell.Caption.Text = item.Title;

            cell.Image.SetImage(new NSUrl(item.ThumbnailLink));

            return cell;
        }
    }
}

