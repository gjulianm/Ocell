﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Ocell.Library;
using TweetSharp;
using Ocell.Library.Twitter;
using Ocell.Pages.Elements;
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;

namespace Ocell
{
    public partial class DMConversation : PhoneApplicationPage
    {
        DMConversationModel viewModel = new DMConversationModel();
        public DMConversation()
        {
            InitializeComponent();

            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };


            Loaded += new RoutedEventHandler(DMConversation_Loaded);
            List.Loader.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsLoading")
                        Dependency.Resolve<IProgressIndicator>().IsLoading = List.Loader.IsLoading;
                };

            DataContext = viewModel;
        }

        void DMConversation_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataTransfer.DMGroup == null)
            {
                NavigationService.GoBack();
                return;
            }

            List.Loader.Source.BulkAdd(DataTransfer.DMGroup.Messages.Cast<ITweetable>());

            string pairName = DataTransfer.DMGroup.Messages.First().GetPairName(DataTransfer.CurrentAccount);

            TwitterResource resource = new TwitterResource
            {
                Type = ResourceType.MessageConversation,
                User = DataTransfer.CurrentAccount,
                Data = pairName
            };

            List.Loader.Resource = resource;
            viewModel.PairName = pairName;
        }
    }
}
