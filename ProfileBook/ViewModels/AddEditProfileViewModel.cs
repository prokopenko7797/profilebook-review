﻿using Plugin.Media.Abstractions;
using Prism.Commands;
using Prism.Navigation;
using ProfileBook.Models;
using ProfileBook.Servcies.ProfileService;
using System;
using Acr.UserDialogs;
using ProfileBook.Servcies.Settings;
using ProfileBook.Constants;
using ProfileBook.Servcies.Authorization;
using ProfileBook.Servcies.Localization;

namespace ProfileBook.ViewModels
{
    public class AddEditProfileViewModel : ViewModelBase
    {
        #region ______Private______

        private string _Title;
        private string _NickName;
        private string _Name;
        private string _ImagePath;
        private string _Description;

        private DelegateCommand _SaveToolBarCommand;
        private DelegateCommand _ImageTapCommand;

        private Profile _profile = new Profile();

        #endregion

        #region ______Services______

        private readonly IProfileService _profileService;
        private readonly IMedia _media;
        private readonly IUserDialogs _userDialogs;
        private readonly IAuthorizationService _authorizationService;

        #endregion


        public AddEditProfileViewModel(INavigationService navigationService, ILocalizationService localizationService, IProfileService profileService,
            IMedia media, IUserDialogs userDialogs, IAuthorizationService authorizationService)
            : base(navigationService, localizationService)
        {

            _profileService = profileService;
            _userDialogs = userDialogs;
            _media = media;
            _authorizationService = authorizationService;

            ImagePath = Constant.DefaultProfileImage;


            
        }


        #region ____Public Properties_____

        public string Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }

        public string NickName
        {
            get { return _NickName; }
            set { SetProperty(ref _NickName, value); }
        }
        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }

        public string Description
        {
            get { return _Description; }
            set { SetProperty(ref _Description, value); }
        }


        public string ImagePath
        {
            get { return _ImagePath; }
            set { SetProperty(ref _ImagePath, value); }
        }

        #endregion

        #region ______Comands_____

        public DelegateCommand SaveToolBarCommand => 
            _SaveToolBarCommand ??
            (_SaveToolBarCommand = new DelegateCommand(ExecuteSaveToolBarAsync));

        public DelegateCommand ImageTapCommand =>
            _ImageTapCommand ??
            (_ImageTapCommand = new DelegateCommand(ExecuteImageTapCommand));

        #endregion

        #region ______Private Helpers________


        private async void ExecuteSaveToolBarAsync()
        {
            if (NickName == default || NickName.Length < 1)
            {
                await _userDialogs.AlertAsync(Resources["NickNameEmpty"], Resources["Error"], Resources["Ok"]);
                return;
            } 
            
            if (_profile.name != Name || _profile.nick_name != NickName
                    || _profile.image_path != ImagePath || _profile.description != Description)
            {

                _profile.name = Name;
                _profile.nick_name = NickName;
                _profile.description = Description;
                _profile.image_path = ImagePath;
                _profile.user_id = _authorizationService.IdUser;
                _profile.date = DateTime.Now;

                
                

                if (_profile.id == default)
                {
                    await _profileService.AddAsync(_profile);
                }
                else
                {
                    await _profileService.EditAsync(_profile);
                }
                


            }
            await NavigationService.GoBackAsync();
        }



        private async void TakeFromGalleryAsync()
        {
            if (_media.IsPickPhotoSupported)
            {
                var image = await _media.PickPhotoAsync(new PickMediaOptions());
                if (image != null)
                {
                    ImagePath = image.Path;
                }
            }
        }
        
        public async void TakeFromCameraAsync()
        {
            if (_media.IsTakePhotoSupported)
            {
                var image = await _media.TakePhotoAsync(new StoreCameraMediaOptions 
                                                        { Name = $"{DateTime.Now }.jpg" });
                if (image != null)
                {
                    ImagePath = image.Path;
                }
            }
        }


        private void ExecuteImageTapCommand()
        {
            _userDialogs.ActionSheet(new ActionSheetConfig()
                .SetTitle("Choose")
                .Add("Camera", TakeFromCameraAsync, "ic_camera_alt_black.png")
                .Add("Gallery", TakeFromGalleryAsync, "ic_collections_black.png")
                .SetCancel("Cancel", null));
        }





        #endregion


        #region ______Overrides________

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetValue<Profile>(nameof(Profile)) != null)
            {
                _profile = parameters.GetValue<Profile>(nameof(Profile));
                Name = _profile.name;
                NickName = _profile.nick_name;
                Description = _profile.description;
                ImagePath = _profile.image_path;

                Title = Resources["EditProfileTitle"];
            }
            else Title = Resources["AddProfileTitle"];

        }



        #endregion

    }
}
