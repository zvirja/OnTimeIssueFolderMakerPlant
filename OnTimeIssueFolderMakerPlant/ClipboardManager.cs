﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OnTimeIssueFolderMakerPlant.OnTime;
using TrayGarden.Helpers;
using TrayGarden.Reception.Services;
using TrayGarden.Services.PlantServices.ClipboardObserver.Core;
using TrayGarden.Services.PlantServices.UserNotifications.Core.UI.ResultDelivering;

namespace OnTimeIssueFolderMakerPlant
{
  public class ClipboardManager : IClipboardListener, IClipboardWorks
  {
    public static ClipboardManager Manager = new ClipboardManager();
    public IClipboardProvider Provider { get; set; }
    public OnTimeValueResolver OnTimeValueResolver { get; set; }

    public ClipboardManager()
    {
      OnTimeValueResolver = new OnTimeValueResolver();
    }


    public void OnClipboardTextChanged(string newClipboardValue)
    {
      if (!Configuration.ActualConfig.ListenClipboard)
        return;
      Tuple<int, string> resolvedValue = OnTimeValueResolver.ResolveOnTimeValue(newClipboardValue);
      if (resolvedValue == null)
        return;
      var resultCode =
        UIConfirmator.ActualConfirmator.ShowConfirmDialog("Create issue folder for {0}({1})?".FormatWith(resolvedValue.Item1, resolvedValue.Item2));
      if (resultCode == ResultCode.PermanentlyClose)
      {
        Configuration.ActualConfig.ListenClipboard = false;
        return;
      }
      if (resultCode != ResultCode.Yes) 
        return;
      var madeFolder = FolderMaker.Maker.MakeFolder(resolvedValue);
      if (madeFolder == null)
      {
        UIConfirmator.ActualConfirmator.ShowInfoDialog("Unable to create folder for {0}({1})".FormatWith(resolvedValue.Item1, resolvedValue.Item2));
        return;
      }
      try
      {
        Process.Start(madeFolder);
      }
      catch
      {
        UIConfirmator.ActualConfirmator.ShowInfoDialog("Unable to open created folder for {0}({1})".FormatWith(resolvedValue.Item1, resolvedValue.Item2));
      }
    }

    public void StoreClipboardValueProvider(IClipboardProvider provider)
    {
      Provider = provider;
    }

  }
}