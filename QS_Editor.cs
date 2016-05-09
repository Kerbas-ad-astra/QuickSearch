﻿/* 
QuickSearch
Copyright 2016 Malah

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>. 
*/

using KSP.UI;
using KSP.UI.Screens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace QuickSearch {
	public partial class QEditor {

		public static QEditor Instance;

		private bool Ready = false;

		public bool isReady {
			get {
				return Ready && searchFilterParts == EditorPartList.Instance.SearchFilterParts;
			}
		}

		public static EditorPartListFilter<AvailablePart> searchFilterParts;

		private Image searchImage = null;

		protected override void Awake() {
			if (HighLogic.LoadedScene != GameScenes.EDITOR) {
				Warning ("The editor search function works only on the on the Editor. Destroy.", "QEditor");
				Destroy (this);
				return;
			}
			if (Instance != null) {
				Warning ("There's already an Instance of " + MOD + ". Destroy.", "QEditor");
				Destroy (this);
				return;
			}
			Instance = this;
			QSettings.Instance.Load ();
			if (!QSettings.Instance.EditorSearch) {
				Warning ("The editor search function is disabled. Destroy.", "QEditor");
				Destroy (this);
				return;
			}
			base.Awake ();
			Log ("Awake", "QEditor");
		}

		protected override void Start() {
			base.Start ();
			Func<AvailablePart, bool> _criteria = (_aPart) => QSearch.FindPart(_aPart);
			searchFilterParts = new EditorPartListFilter<AvailablePart> (MOD, _criteria);
			PartCategorizer.Instance.searchField.onValueChange.RemoveAllListeners ();
			PointerClickHandler _pointerClickSearch = null;
			PartCategorizer.Instance.searchField.GetComponentCached<PointerClickHandler> (ref _pointerClickSearch);
			if (_pointerClickSearch != null) {
				_pointerClickSearch.onPointerClick.RemoveAllListeners ();
				_pointerClickSearch.onPointerClick.AddListener (new UnityAction<PointerEventData> (SearchField_OnClick));
			}
			PartCategorizer.Instance.searchField.onEndEdit.AddListener (new UnityAction<string> (SearchField_OnEndEdit));
			PartCategorizer.Instance.searchField.onValueChange.AddListener (new UnityAction<string> (SearchField_OnValueChange));
			PartCategorizer.Instance.searchField.GetComponentCached<Image> (ref searchImage);
			GameSettings.Editor_partSearch.inputLockMask = QSettings.idSearchKey;
			InputLockManager.SetControlLock ((ControlTypes)QSettings.idSearchKey, MOD + "SearchKey");
			setSearchFilter ();
			Log ("Start", "QEditor");
		}

		private void Update() {
			if (GameSettings.Editor_partSearch.IsUnlocked ()) {
				return;
			}
			if (!isReady) {
				return;
			}
			if (Input.GetKeyDown (GameSettings.Editor_partSearch.primary) || Input.GetKeyDown (GameSettings.Editor_partSearch.secondary)) {
				InitSearch ();
			}
		}

		protected override void OnDestroy() {
			base.OnDestroy ();
			InputLockManager.RemoveControlLock (MOD + "SearchKey");
			Log ("OnDestroy", "QEditor");
		}

		private void SearchField_OnClick(PointerEventData eventData) {
			if (!Ready) {
				return;
			}
			InitSearch ();
			Log ("SearchField_OnClick", "QEditor");
		}

		private void InitSearch() {
			PartCategorizer.Instance.FocusSearchField ();
			if (searchImage != null) {
				searchImage.color = Color.cyan;
			}
			setSearchFilter();
			EditorPartList.Instance.Refresh (EditorPartList.State.PartSearch);
			Log ("InitSearch", "QEditor");
		}

		private void SearchField_OnValueChange(string s) {
			if (!isReady) {
				return;
			}
			QSearch.Text = s;
			EditorPartList.Instance.Refresh ();
			Log ("SearchField_OnValueChange: " + s, "QEditor");
		}

		private void SearchField_OnEndEdit(string s) {
			if (!isReady) {
				return;
			}
			Log ("SearchField_OnEndEdit", "QEditor");
		}

		public void Refresh() {
			QSearch.Text = PartCategorizer.Instance.searchField.text;
			setSearchFilter ();
			EditorPartList.Instance.Refresh ();
		}

		private void setSearchFilter() {
			EditorPartList.Instance.SearchFilterParts = searchFilterParts;
			Ready = true;
			Log ("setSearchFilter", "QEditor");
		}

		private void resetSearchFilter() {
			EditorPartList.Instance.SearchFilterParts = null;
			Log ("resetSearchFilter", "QEditor");
		}
	}
}