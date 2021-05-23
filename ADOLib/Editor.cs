using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using ADOLib.Misc;
using Object = System.Object;

namespace ADOLib
{
    public static class Editor
    {
        public static void ShowPopup(bool show, scnEditor.PopupType popupType, string text = "Example Text", float y = 450f,
            float alpha = 0.5f, bool skipAnim = false)
        {
            scnEditor editor = scnEditor.instance;
            if (editor == null)
            {
                foreach (var obj in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    var e = obj.GetComponent<scnEditor>();
                    if (e == null) e = obj.GetComponentInChildren<scnEditor>();
                    if (e == null) break;
                    editor = e;
                }
            }
            

            if (editor.get<bool>("popupIsAnimating"))
                return;
            editor.set<bool>("showingPopup", show);
            if (show)
            {
                foreach (Component component in editor.popupWindow.transform)
                    component.gameObject.SetActive(false);
                switch (popupType)
                {
                    case scnEditor.PopupType.SaveBeforeSongImport:
                    case scnEditor.PopupType.SaveBeforeImageImport:
                    case scnEditor.PopupType.SaveBeforeVideoImport:
                    case scnEditor.PopupType.SaveBeforeLevelExport:
                        editor.savePopupContainer.SetActive(true);
                        editor.savePopupText.text = text;
                        break;
                    case scnEditor.PopupType.ExportLevel:
                        editor.publishWindow.windowContainer.SetActive(true);
                        editor.publishWindow.Init();
                        editor.invoke<object>("ShowEventPicker", new object[] {false});
                        editor.settingsPanel.ShowInspector(false);
                        editor.levelEventsPanel.ShowInspector(false);
                        break;
                    case scnEditor.PopupType.MissingExportParams:
                        editor.paramsPopupContainer.SetActive(true);
                        string str2 = text;
                        //editor.paramsPopupText.text =
                        //    str2.Replace("[artist]", "<b>" + editor.levelData.artist + "</b>");
                        break;
                    case scnEditor.PopupType.OpenURL:
                        editor.urlPopupContainer.SetActive(true);
                        break;
                    case scnEditor.PopupType.CopyrightWarning:
                        editor.invoke<object>("ShowEventPicker", new object[] {false});
                        editor.settingsPanel.ShowInspector(false);
                        editor.levelEventsPanel.ShowInspector(false);
                        editor.copyrightPopupContainer.SetActive(true);
                        editor.copyrightText.text = text;
                        break;
                    case scnEditor.PopupType.OggEncode:
                        editor.oggPopupContainer.SetActive(true);
                        editor.popupOggCancel.interactable = true;
                        editor.popupOggConvert.interactable = true;
                        editor.oggConversionBar.gameObject.SetActive(false);
                        editor.oggConversionBarText.text = text;
                        break;
                    case scnEditor.PopupType.ConversionSuccesful:
                        editor.okPopupContainer.SetActive(true);
                        editor.okPopupText.text = text;
                        break;
                    case scnEditor.PopupType.ConversionError:
                        editor.okPopupContainer.SetActive(true);
                        editor.okPopupText.text = text;
                        break;
                    case scnEditor.PopupType.UnsavedChanges:
                        editor.unsavedChangesPopupContainer.SetActive(true);
                        break;
                }
            }

            editor.popupPanel.SetActive(true);
            Image component1 = editor.popupPanel.GetComponent<Image>();
            RectTransform component2 = editor.popupWindow.GetComponent<RectTransform>();
            float duration = skipAnim ? 0.0f : 0.5f;
            alpha = show ? alpha : 0.0f;
            float num = 20f;
            float endValue = show ? num : y;
            component2.DOKill();
            component1.DOKill();
            if (show)
            {
                component2.SetAnchorPosY(y);
                component1.color = Color.black.WithAlpha(0.0f);
            }

            DOTweenModuleUI.DOColor(component1, Color.black.WithAlpha(alpha), duration / 2f)
                .SetUpdate<TweenerCore<Color, Color, ColorOptions>>(true)
                .SetEase<TweenerCore<Color, Color, ColorOptions>>(Ease.Linear);
            editor.set<bool>("popupIsAnimating", true);
            component2.DOAnchorPosY(endValue, duration).SetUpdate<TweenerCore<Vector2, Vector2, VectorOptions>>(true)
                .SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(show ? Ease.OutBack : Ease.InBack)
                .OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>((TweenCallback) (() =>
                {
                    editor.set<bool>("popupIsAnimating", false);
                    if (show)
                        return;
                    editor.popupPanel.SetActive(false);
                }));
            editor.ShowFileActionsPanel(false);
            editor.ShowShortcutsPanel(false);
        }
    }
}