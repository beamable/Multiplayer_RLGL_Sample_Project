using BeamableExample.Helpers;
using System;
using TMPro;
using UnityEngine;

namespace BeamableExample.UI
{
	public class ErrorBox : SingletonNonPersistant<ErrorBox>
	{
		[SerializeField] private TextMeshProUGUI _header;
		[SerializeField] private TextMeshProUGUI _message;

		private Action _onClose;
		private CanvasGroup _panel;

		public static void Show(string header, string message, Action onclose)
		{
			Instance.ShowInternal(header, message, onclose);
		}

		private void ShowInternal(string header, string message, Action onclose)
		{
			_header.text = header;
			_message.text = message;
			_onClose = onclose;
			if(_panel==null)
				_panel = GetComponent<CanvasGroup>();
			SetVisible(true);
		}

		public void SetVisible(bool value)
        {
			_panel.alpha = (value == true) ? 1.0f : 0.0f;
			_panel.interactable = value;
			_panel.blocksRaycasts = value;
		}

		public void OnClose()
		{
			SetVisible(false);
			_onClose();
		}
	}
}