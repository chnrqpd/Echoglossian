﻿// <copyright file="PluginUI.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

using Echoglossian.Properties;
using ImGuiNET;

namespace Echoglossian;
public partial class Echoglossian
{
  // public string[] FontSizes = Array.ConvertAll(Enumerable.Range(4, 72).ToArray(), x => x.ToString());
  private List<string> languageList;

  private List<string> enginesList = new()
  {
    "Google Translate",
    "DeepL",
    "ChatGPT",
  };

  private void EchoglossianConfigUi()
  {
    var saveConfig = false;
    this.languageList = new List<string>();

    foreach (var l in this.languagesDictionary)
    {
      this.languageList.Add(l.Value.LanguageName);
    }

    ImGui.SetNextWindowSizeConstraints(new Vector2(900, 700), new Vector2(1920, 1080));

    ImGui.Begin($"{Resources.ConfigWindowTitle} - Plugin Version: {this.configuration.PluginVersion}", ref this.config);

    ImGui.BeginGroup();

    UINewFontHandler.GeneralFontHandle.Push();

    var langToRemoveDiacritics = this.configuration.Lang is 24 or 25 or 44 or 60 or 61 or 80 or 83 or 87 or 91 or 104 or 105 or 109 or 110;

    if (ImGui.Combo(Resources.LanguageSelectLabelText, ref languageInt, this.languageList.ToArray(), this.languageList.ToArray().Length))
    {
      this.configuration.Lang = languageInt;
      SpecialFontFileName = langDict[this.configuration.Lang].FontName;
      SelectedLanguage = this.languagesDictionary[this.configuration.Lang];

      var languageNotSupported = this.configuration.Lang is 2 or 3 or 5 or 6 or 11 or 13 or 40 or 42 or 57 or 78 or 82 or 106 or 108 or 111 or 112 or 116;
      var languageOnlySupportedThruOverlay = this.configuration.Lang is 4 or 8 or 9 or 10 or 12 or 14 or 15 or 16 or 18 or 19 or 21 or 22 /*or 24 or 25*/ or 29 or 35 or 37 or 38 or 41 or 43 or 45 or 46 or 51 or 52 or 53 or 55 or 56 or 58 /*or 60 or 61*/ or 64 or 67 or 69 or 70 or 71 or 72 or 76 or 77 /*or 80 or 83 */ or 85 or 86 or 89 or 90 /*or 91*/ or 92 or 99 or 100 or 101 or 102 or 103 /*or 104 or 105*/ or 107/* or 109 or 110*/;


      if (languageNotSupported)
      {
        this.configuration.UnsupportedLanguage = true;
      }
      else
      {
        this.configuration.UnsupportedLanguage = false;
        this.configuration.OverlayOnlyLanguage = languageOnlySupportedThruOverlay;
      }

      if (!langDict[languageInt].SupportedEngines.Contains(this.configuration.ChosenTransEngine))
      {
        // use Google Translate as default
        this.configuration.ChosenTransEngine = 0;
        this.translationService = new TranslationService(this.configuration, PluginLog, sanitizer);
      }

      saveConfig = true;

      PluginLog.Debug("Language selected: " + langDict[this.configuration.Lang].LanguageName);

      PluginLog.Debug("Language font: " + langDict[this.configuration.Lang].FontName);

      MountFontPaths();
      PluginInterface.UiBuilder.FontAtlas.BuildFontsAsync();
    }

    Echoglossian.UINewFontHandler.GeneralFontHandle.Pop();

    ImGui.SameLine();
    ImGui.Text(Resources.HoverTooltipIndicator);
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip(Resources.LanguageSelectionTooltip);
    }

    if (this.configuration.UnsupportedLanguage)
    {
      ImGui.Text(Resources.LanguageNotSupportedText);
      saveConfig |= AssignIfChanged(ref this.configuration.Translate, false);
    }

    if (this.configuration.OverlayOnlyLanguage)
    {
      ImGui.Text(Resources.LanguageOnlySupportedUsingOverlay);
    }

    ImGui.EndGroup();
    ImGui.Spacing();

    if (!this.configuration.UnsupportedLanguage)
    {
      saveConfig |= ImGui.Checkbox(Resources.EnableTranslation, ref this.configuration.Translate);
    }

    if (this.configuration.Translate)
    {
      ImGui.SameLine();
      ImGui.TextColored(new Vector4(0, 255, 0, 255), Resources.TranslationEnabled);
    }
    else
    {
      ImGui.SameLine();
      ImGui.TextColored(new Vector4(255, 255, 0, 255), Resources.TranslationDisabled);
    }

    if (this.configuration.Translate)
    {
      ImGui.BeginGroup();

      ImGui.Text(Resources.WhatToTranslateText);

      ImGui.EndGroup();
    }

    ImGui.Spacing();

    if (ImGui.BeginTabBar(
      "TabBar",
      ImGuiTabBarFlags.NoCloseWithMiddleMouseButton))
    {
      if (ImGui.BeginTabItem(Resources.ConfigTab1Name))
      {
        /* - talk - */
        if (this.configuration.Translate)
        {
          saveConfig |= ImGui.Checkbox(
            Resources.TranslateTalkToggleLabel,
            ref this.configuration.TranslateTalk);

          if (this.configuration.TranslateTalk)
          {
            if (this.configuration.OverlayOnlyLanguage)
            {
              saveConfig |=
                AssignIfChanged(ref this.configuration.UseImGuiForTalk, true);
              saveConfig |=
                AssignIfChanged(
                  ref this.configuration.SwapTextsUsingImGui,
                  false);
            }
            else
            {
              saveConfig |= ImGui.Checkbox(
                Resources.OverlayToggleLabel,
                ref this.configuration.UseImGuiForTalk);
            }

            saveConfig |= ImGui.Checkbox(
              Resources.TranslateNpcNamesToggle,
              ref this.configuration.TranslateNpcNames);

            ImGui.Spacing();
            ImGui.Separator();

            if (this.configuration.UseImGuiForTalk)
            {
              ImGui.Text(Resources.ImguiAdjustmentsLabel);
              if (ImGui.SliderFloat(
                Resources.OverlayFontScaleLabel,
                ref this.configuration.FontScale, -3f, 3f, "%.2f"))
              {
                saveConfig = true;
                this.configuration.FontChangeTime = DateTime.Now.Ticks;
              }

              ImGui.SameLine();
              ImGui.Text(Resources.HoverTooltipIndicator);
              if (ImGui.IsItemHovered())
              {
                ImGui.SetTooltip(Resources.OverlayFontSizeOrientations);
              }

              ImGui.Text(Resources.FontColorSelectLabel);
              ImGui.SameLine();
              saveConfig |= ImGui.ColorEdit3(
                Resources.OverlayColorSelectName,
                ref this.configuration.OverlayTalkTextColor,
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);

              ImGui.SameLine();
              ImGui.Text(Resources.HoverTooltipIndicator);
              if (ImGui.IsItemHovered())
              {
                ImGui.SetTooltip(Resources.OverlayFontColorOrientations);
              }

              ImGui.Spacing();
              ImGui.Separator();
              saveConfig |= ImGui.DragFloat(
                Resources.OverlayWidthScrollLabel,
                ref this.configuration.ImGuiTalkWindowWidthMult, 0.001f, 0.01f,
                3f);

              ImGui.Separator();
              saveConfig |= ImGui.DragFloat(
                Resources.OverlayHeightScrollLabel,
                ref this.configuration.ImGuiTalkWindowHeightMult, 0.001f, 0.01f,
                3f);

              ImGui.Separator();
              ImGui.Spacing();
              saveConfig |= ImGui.DragFloat2(
                Resources.OverlayPositionAdjustmentLabel,
                ref this.configuration.ImGuiWindowPosCorrection);

              ImGui.SameLine();
              ImGui.Text(Resources.HoverTooltipIndicator);
              if (ImGui.IsItemHovered())
              {
                ImGui.SetTooltip(Resources.OverlayAdjustmentOrientations);
              }
            }

            ImGui.Spacing();
            ImGui.Separator();
            if (!this.configuration.OverlayOnlyLanguage &&
                this.configuration.UseImGuiForTalk)
            {
              saveConfig |= ImGui.Checkbox(
                Resources.SwapTranslationTextToggle,
                ref this.configuration.SwapTextsUsingImGui);

              if (this.configuration.SwapTextsUsingImGui && langToRemoveDiacritics)
              {
                saveConfig |= ImGui.Checkbox(
                                   Resources.RemoveDiacriticsToggle,
                                   ref this.configuration.RemoveDiacriticsWhenUsingReplacementTalkBTalk);
              }
            }
          }
        }

        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem(Resources.ConfigTab2Name))
      {
        if (this.configuration.Translate)
        {
          /* - battle talk - */
          saveConfig |= ImGui.Checkbox(
            Resources.TransLateBattletalkToggle,
            ref this.configuration.TranslateBattleTalk);

          ImGui.BeginGroup();
          /*if (this.configuration.OverlayOnlyLanguage)
          {
            this.configuration.TranslateBattleTalk = false; // had disabled so no texts are lost while we fix this
            saveConfig |=
              AssignIfChanged(
                ref this.configuration.UseImGuiForBattleTalk,
                true);
          }
          else
          {
            this.configuration.UseImGuiForBattleTalk = false; // had disabled so no texts are lost while we fix this
            saveConfig |= ImGui.Checkbox(
              Resources.OverlayToggleLabel,
              ref this.configuration.UseImGuiForBattleTalk);
          }*/

          saveConfig |= ImGui.Checkbox(
            Resources.TranslateNpcNamesToggle,
            ref this.configuration.TranslateNpcNames);

          ImGui.EndGroup();
        }

        ImGui.Spacing();
        ImGui.Separator();

        if (this.configuration.UseImGuiForBattleTalk)
        {
          ImGui.Text(Resources.ImguiAdjustmentsLabel);
          if (ImGui.SliderFloat(
            Resources.OverlayFontScaleLabel,
            ref this.configuration.BattleTalkFontScale, -3f, 3f, "%.2f"))
          {
            saveConfig = true;
            this.configuration.FontChangeTime = DateTime.Now.Ticks;
          }

          ImGui.SameLine();
          ImGui.Text(Resources.HoverTooltipIndicator);
          if (ImGui.IsItemHovered())
          {
            ImGui.SetTooltip(Resources.OverlayFontSizeOrientations);
          }

          ImGui.Spacing();
          ImGui.Text(Resources.FontColorSelectLabel);
          ImGui.SameLine();
          saveConfig |= ImGui.ColorEdit3(
            Resources.OverlayColorSelectName,
            ref this.configuration.OverlayBattleTalkTextColor,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);

          ImGui.SameLine();
          ImGui.Text(Resources.HoverTooltipIndicator);
          if (ImGui.IsItemHovered())
          {
            ImGui.SetTooltip(Resources.OverlayFontColorOrientations);
          }

          ImGui.Spacing();
          ImGui.Separator();
          saveConfig |= ImGui.DragFloat(
            Resources.OverlayWidthScrollLabel,
            ref this.configuration.ImGuiBattleTalkWindowWidthMult, 0.001f,
            0.01f, 3f);

          ImGui.Separator();
          saveConfig |= ImGui.DragFloat(
            Resources.OverlayHeightScrollLabel,
            ref this.configuration.ImGuiBattleTalkWindowHeightMult, 0.001f,
            0.01f, 3f);

          ImGui.Separator();
          ImGui.Spacing();
          saveConfig |= ImGui.DragFloat2(
            Resources.OverlayPositionAdjustmentLabel,
            ref this.configuration.ImGuiBattleTalkWindowPosCorrection);

          ImGui.SameLine();
          ImGui.Text(Resources.HoverTooltipIndicator);
          if (ImGui.IsItemHovered())
          {
            ImGui.SetTooltip(Resources.OverlayAdjustmentOrientations);
          }
        }

        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem(Resources.ConfigTab3Name))
      {
        if (this.configuration.Translate)
        {
          /* - Toast messages - */
          saveConfig |= ImGui.Checkbox(
            Resources.TranslateToastToggleText,
            ref this.configuration.TranslateToast);

          ImGui.BeginGroup();
          if (this.configuration.OverlayOnlyLanguage)
          {
            saveConfig |=
              AssignIfChanged(ref this.configuration.UseImGuiForToasts, true);
          }
          else
          {
            saveConfig |= ImGui.Checkbox(
              Resources.UseImGuiForToastsToggle,
              ref this.configuration.UseImGuiForToasts);
          }
        }

        ImGui.EndGroup();

        if (this.configuration.TranslateToast)
        {
          ImGui.Separator();
          ImGui.Text(Resources.WhichToastsToTranslate);
          saveConfig |= ImGui.Checkbox(
            Resources.TranslateErrorToastToggleText,
            ref this.configuration.TranslateErrorToast);
          saveConfig |= ImGui.Checkbox(
            Resources.TranslateQuestToastToggleText,
            ref this.configuration.TranslateQuestToast);
          saveConfig |= ImGui.Checkbox(
            Resources.TranslateAreaToastToggleText,
            ref this.configuration.TranslateAreaToast);
          saveConfig |=
            ImGui.Checkbox(
              Resources.TranslateClassChangeToastToggleText,
              ref this.configuration.TranslateClassChangeToast);
          saveConfig |=
            ImGui.Checkbox(
              Resources.TranslateScreenInfoToastToggleText,
              ref this.configuration.TranslateWideTextToast);
        }

        ImGui.Separator();
        if (this.configuration.UseImGuiForToasts)
        {
          ImGui.Text(Resources.ImguiAdjustmentsLabel);

          saveConfig |= ImGui.DragFloat(
            Resources.ToastOverlayWidthScrollLabel,
            ref this.configuration.ImGuiToastWindowWidthMult, 0.001f, 0.01f,
            3f);

          ImGui.SameLine();
          ImGui.Text(Resources.HoverTooltipIndicator);
          if (ImGui.IsItemHovered())
          {
            ImGui.SetTooltip(Resources.ToastOverlayWidthMultiplierOrientations);
          }
        }

        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem(Resources.ConfigTab4Name))
      {
        if (this.configuration.Translate)
        {
          /* - Journal - */
          saveConfig |= ImGui.Checkbox(
            Resources.TranslateJournalToggle,
            ref this.configuration.TranslateJournal);
        }

        if (langToRemoveDiacritics)
        {
          saveConfig |= ImGui.Checkbox(
                             Resources.RemoveDiacriticsToggle,
                             ref this.configuration.RemoveDiacriticsWhenUsingReplacementQuest);
        }

        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem(Resources.ConfigTab5Name))
      {
        /* - talk subtitle - */
        if (this.configuration.Translate)
        {
          saveConfig |= ImGui.Checkbox(
            Resources.TranslateTalkSubtitleToggleLabel,
            ref this.configuration.TranslateTalkSubtitle);

          if (this.configuration.TranslateTalkSubtitle)
          {
            if (this.configuration.OverlayOnlyLanguage)
            {
              saveConfig |=
                AssignIfChanged(ref this.configuration.UseImGuiForTalkSubtitle, true);
              saveConfig |=
                AssignIfChanged(
                  ref this.configuration.SwapTextsUsingImGui,
                  false);
            }
            else
            {
              saveConfig |= ImGui.Checkbox(
                Resources.OverlayToggleLabel,
                ref this.configuration.UseImGuiForTalkSubtitle);
            }

            ImGui.Spacing();
            ImGui.Separator();

            if (this.configuration.UseImGuiForTalkSubtitle)
            {
              ImGui.Text(Resources.ImguiAdjustmentsLabel);
              if (ImGui.SliderFloat(
                Resources.OverlayFontScaleLabel,
                ref this.configuration.FontScale, -3f, 3f, "%.2f"))
              {
                saveConfig = true;
                this.configuration.FontChangeTime = DateTime.Now.Ticks;
              }

              ImGui.SameLine();
              ImGui.Text(Resources.HoverTooltipIndicator);
              if (ImGui.IsItemHovered())
              {
                ImGui.SetTooltip(Resources.OverlayFontSizeOrientations);
              }

              ImGui.Text(Resources.FontColorSelectLabel);
              ImGui.SameLine();
              saveConfig |= ImGui.ColorEdit3(
                Resources.OverlayColorSelectName,
                ref this.configuration.OverlayTalkTextColor,
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);

              ImGui.SameLine();
              ImGui.Text(Resources.HoverTooltipIndicator);
              if (ImGui.IsItemHovered())
              {
                ImGui.SetTooltip(Resources.OverlayFontColorOrientations);
              }

              ImGui.Spacing();
              ImGui.Separator();
              saveConfig |= ImGui.DragFloat(
                Resources.OverlayWidthScrollLabel,
                ref this.configuration.ImGuiTalkSubtitleWindowWidthMult, 0.001f, 0.01f,
                3f);

              ImGui.Separator();
              saveConfig |= ImGui.DragFloat(
                Resources.OverlayHeightScrollLabel,
                ref this.configuration.ImGuiTalkSubtitleWindowHeightMult, 0.001f, 0.01f,
                3f);

              ImGui.Separator();
              ImGui.Spacing();
              saveConfig |= ImGui.DragFloat2(
                Resources.OverlayPositionAdjustmentLabel,
                ref this.configuration.ImGuiTalkSubtitleWindowPosCorrection);

              ImGui.SameLine();
              ImGui.Text(Resources.HoverTooltipIndicator);
              if (ImGui.IsItemHovered())
              {
                ImGui.SetTooltip(Resources.OverlayAdjustmentOrientations);
              }
            }

            ImGui.Spacing();
            ImGui.Separator();
            if (!this.configuration.OverlayOnlyLanguage &&
                this.configuration.UseImGuiForTalkSubtitle)
            {
              saveConfig |= ImGui.Checkbox(
                Resources.SwapTranslationTextToggle,
                ref this.configuration.SwapTextsUsingImGui);
            }

            if (this.configuration.SwapTextsUsingImGui && langToRemoveDiacritics)
            {
              saveConfig |= ImGui.Checkbox(
                                 Resources.RemoveDiacriticsToggle,
                                 ref this.configuration.RemoveDiacriticsWhenUsingReplacementTalkBTalk);
            }
          }
        }

        ImGui.EndTabItem();
      }

      /*if (ImGui.BeginTabItem(Resources.ConfigTab6Name))
      {
        ImGui.Text("This is the Onion tab!\nblah blah blah blah blah");
        ImGui.EndTabItem();
      }*/

      if (ImGui.BeginTabItem(Resources.ConfigTab7Name))
      {
        ImGui.Checkbox(Resources.TranslateTextsAgain, ref this.configuration.TranslateAlreadyTranslatedTexts);

        var engines = this.enginesList.Where((_, i) => langDict[languageInt].SupportedEngines.Contains(i)).ToArray();
        if (ImGui.Combo(Resources.TranslationEngineChoose, ref chosenTransEngine, engines, engines.Length))
        {
          this.configuration.ChosenTransEngine = chosenTransEngine;
          this.translationService = new TranslationService(this.configuration, PluginLog, sanitizer);
        }

        ImGui.BeginGroup();
        switch (this.configuration.ChosenTransEngine)
        {
          case 0:
            ImGui.TextWrapped(Resources.SettingsForGTransText);
            ImGui.TextWrapped(Resources.TranslationEngineSettingsNotRequired);
            break;
          case 1:
            ImGui.TextWrapped(Resources.SettingsForDeepLTransText);
            ImGui.Spacing();

            var isDeeplTranslatorUsingApiKey = this.configuration.DeeplTranslatorUsingApiKey;
            if (ImGui.Checkbox(Resources.DeepLTransAPIKey, ref isDeeplTranslatorUsingApiKey))
            {
              this.configuration.DeeplTranslatorUsingApiKey = isDeeplTranslatorUsingApiKey;
              this.translationService = new TranslationService(this.configuration, PluginLog, sanitizer);
            }

            if (this.configuration.DeeplTranslatorUsingApiKey)
            {
              if (ImGui.Button(Resources.DeepLTranslatorAPIKeyLink))
              {
                saveConfig = true;
                Process.Start(new ProcessStartInfo
                {
                  FileName = "https://www.deepl.com/pro-api",
                  UseShellExecute = true,
                });
                this.config = false;
              }

              ImGui.Spacing();

              if (ImGui.InputText(Resources.DeeplTranslatorApiKey, ref this.configuration.DeeplTranslatorApiKey, 100))
              {
                this.translationService = new TranslationService(this.configuration, PluginLog, sanitizer);
              }
            }

            break;
          case 2:
            ImGui.TextWrapped(Resources.SettingsForChatGptTransText);
            ImGui.Spacing();
            if (ImGui.Button(Resources.ChatGPTAPIKeyLink))
            {
              saveConfig = true;
              Process.Start(new ProcessStartInfo
              {
                FileName = "https://platform.openai.com/settings/profile?tab=api-keys",
                UseShellExecute = true,
              });
              this.config = false;
            }

            ImGui.Spacing();

            if (ImGui.InputText(Resources.ChatGptApiKey, ref this.configuration.ChatGptApiKey, 400))
            {
              this.translationService = new TranslationService(this.configuration, PluginLog, sanitizer);
            }

            break;
        }

        ImGui.EndGroup();
        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem(Resources.ConfigTab8Name))
      {
        var pluginAssetsStatus = this.configuration.PluginAssetsDownloaded;

        ImGui.BeginGroup();
        /*if (pluginAssetsStatus)
        {
          // TODO: Add a button to re-download the assets
          ImGui.Text(Resources.PluginAssetsDownloadedText);
        }
        else
        {*/
        ImGui.TextWrapped(Resources.CurrentPluginAssetsStatus + ": " + pluginAssetsStatus);
        ImGui.TextWrapped(Resources.PluginAssetsNotDownloadedText);
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        if (ImGui.Button(Resources.DownloadPluginAssetsButtonText))
        {
          this.DownloadAssets(0);
          this.DownloadAssets(1);
          this.DownloadAssets(2);
          this.DownloadAssets(3);
          this.DownloadAssets(4);
          this.PluginAssetsChecker();
          saveConfig = true;
        }

        ImGui.PopStyleColor(3);

        ImGui.EndGroup();
        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem(Resources.ConfigTab9Name))
      {
        ImGui.Text(Resources.ConfigTab9Text);

        ImGui.Checkbox(Resources.ConfigTab9CheckboxClipboardText, ref this.configuration.CopyTranslationToClipboard);
        ImGui.TextWrapped(Resources.ConfigTab9CheckboxClipboardTooltipText);

        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem(Resources.ConfigTabAbout))
      {
        if (ImGui.BeginTable("columns", 2))
        {
          ImGui.TableNextColumn();
          ImGui.BeginGroup();
          ImGui.TextColored(new Vector4(247, 247, 7, 255), Resources.DisclaimerTitle);
          ImGui.Spacing();
          ImGui.TextWrapped(Resources.DisclaimerText1);
          ImGui.TextWrapped(Resources.DisclaimerText2);
          ImGui.TextWrapped(Resources.ContribText);
          ImGui.EndGroup();

          ImGui.TableNextColumn();
          var posLogo = new Vector2(ImGui.GetWindowContentRegionMax().X - 300, ImGui.GetWindowContentRegionMin().Y + 150);
          ImGui.SetCursorPos(posLogo);
          ImGui.Image(this.logo.ImGuiHandle, new Vector2(300, 300));
          ImGui.EndTable();
        }

        ImGui.EndTabItem();
      }

      ImGui.EndTabBar();
    }

    ImGui.Spacing();
    var pos = new Vector2(ImGui.GetWindowContentRegionMin().X, ImGui.GetWindowContentRegionMax().Y - 100);

    ImGui.SetCursorPos(pos);
    ImGui.Separator();
    ImGui.BeginGroup();
    ImGui.TextWrapped(Resources.NEListText);

    ImGui.PushID(1);
    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1 / 7.0f, 0.6f, 0.6f, 1f));
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1 / 7.0f, 0.7f, 0.7f, 1f));
    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1 / 7.0f, 0.8f, 0.8f, 1f));
    if (ImGui.Button(Resources.TodoUrl))
    {
      saveConfig = true;
      Process.Start(new ProcessStartInfo
      {
        FileName = "https://github.com/users/lokinmodar/projects/2",
        UseShellExecute = true,
      });
      this.config = false;
    }

    ImGui.PopStyleColor(3);
    ImGui.PopID();

    ImGui.Spacing();

    if (ImGui.Button(Resources.SaveCloseButtonLabel))
    {
      saveConfig = true;
      this.config = false;
    }

    ImGui.SameLine();
    ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
    ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
    if (ImGui.Button(Resources.PatronButtonLabel))
    {
      saveConfig = true;
      Process.Start(new ProcessStartInfo
      {
        FileName = "https://ko-fi.com/lokinmodar",
        UseShellExecute = true,
      });
      this.config = false;
    }

    ImGui.PopStyleColor(3);
    ImGui.SameLine();
    ImGui.PushID(4);
    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(4, 7.0f, 0.6f, 0.6f));
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(4, 7.0f, 0.7f, 0.7f));
    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(4, 7.0f, 0.8f, 0.8f));
    if (ImGui.Button(Resources.SendPixButton))
    {
      saveConfig = true;
      ImGui.OpenPopup(Resources.PixQrWindowLabel);
    }

    // Always center this window when appearing
    var center = ImGui.GetMainViewport().GetCenter();
    ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
    if (ImGui.BeginPopupModal(Resources.PixQrWindowLabel))
    {
      ImGui.Text(Resources.QRCodeInstructionsText);
      ImGui.Image(this.pixImage.ImGuiHandle, new Vector2(512, 512));
      if (ImGui.Button(Resources.CloseButtonLabel))
      {
        ImGui.CloseCurrentPopup();
      }

      ImGui.EndPopup();
      ImGui.SetItemDefaultFocus();
    }

    ImGui.PopStyleColor(3);
    ImGui.PopID();
    ImGui.EndGroup();
    ImGui.End();

    if (saveConfig)
    {
      this.SaveConfig();
    }
  }

  private bool DisableAllToastTranslations()
  {
    this.configuration.TranslateAreaToast = false;
    this.configuration.TranslateClassChangeToast = false;
    this.configuration.TranslateErrorToast = false;
    this.configuration.TranslateQuestToast = false;
    this.configuration.TranslateWideTextToast = false;
    this.SaveConfig();
    return true;
  }
}