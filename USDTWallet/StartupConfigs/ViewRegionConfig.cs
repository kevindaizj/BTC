﻿using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USDTWallet.Views.BTC;
using USDTWallet.Views.Home;
using USDTWallet.Views.Mempool;
using USDTWallet.Views.Tools;
using USDTWallet.Views.TransactionOverview;
using USDTWallet.Views.USDT;

namespace USDTWallet.StartupConfigs
{
    public static class ViewRegionConfig
    {
        public static void RegisterViewNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HomePage>();
            containerRegistry.RegisterForNavigation<BTCPage>();
            containerRegistry.RegisterForNavigation<USDTPage>();
            containerRegistry.RegisterForNavigation<TransactionOverviewPage>();
            containerRegistry.RegisterForNavigation<Mempool>();
            containerRegistry.RegisterForNavigation<ToolPage>();
        }

        public static void InitViewWithRegion(IRegionManager regionManager)
        {
            regionManager.RegisterViewWithRegion(WalletRegions.GlobalRegion, typeof(HomePage));
            regionManager.RegisterViewWithRegion(WalletRegions.GlobalRegion, typeof(BTCPage));
            regionManager.RegisterViewWithRegion(WalletRegions.GlobalRegion, typeof(USDTPage));
            regionManager.RegisterViewWithRegion(WalletRegions.GlobalRegion, typeof(TransactionOverviewPage));
            regionManager.RegisterViewWithRegion(WalletRegions.GlobalRegion, typeof(Mempool));
            regionManager.RegisterViewWithRegion(WalletRegions.GlobalRegion, typeof(ToolPage));
        }

        public static class WalletRegions
        {
            public static readonly string GlobalRegion = "MainContentRegion";
        }
    }
}
