﻿/* 
QuickSearch
Copyright 2015 Malah

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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuickSearch {
	public class QCategorizer {

		internal static string IconTexturePath = Quick.MOD + "/Textures/icon";
		internal static string IconSelectedTexturePath = Quick.MOD + "/Textures/iconSelected";
		internal static Texture2D IconTexture;
		internal static Texture2D IconSelectedTexture;
		internal static PartCategorizer.Icon Icon;

		internal static PartCategorizer.Category FilterPartSearch;
		internal static PartCategorizer.Category SubCategoryPartSearch;
		internal static PartCategorizer.Category CurrentFilter;
		internal static PartCategorizer.Category CurrentSubCategory;
		internal static PartCategorizer.Category lastFilter;
		internal static PartCategorizer.Category lastSubCategory;
		internal static bool lastIsAdvancedMode = false;

		// Lister tous les filtres et categories
		internal static List<PartCategorizer.Category> Filters {
			get {
				List<PartCategorizer.Category> _filters = new List<PartCategorizer.Category>();
				_filters.AddRange(PartCategorizer.Instance.filters);
				_filters.AddRange(PartCategorizer.Instance.categories);
				return _filters;
			}
		}

		// Indiquer quel filtre est sélectionné
		internal static PartCategorizer.Category FilterSelected {
			get {
				return Filters.Find (f => f.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE);
			}
		}

		// Indiquer quel catégorie est sélectionnée
		internal static PartCategorizer.Category CategorySelected() {
			return CategorySelected (FilterSelected);
		}

		internal static PartCategorizer.Category CategorySelected(PartCategorizer.Category currentFilter) {
			List<PartCategorizer.Category> _subcategories = currentFilter.subcategories;
			return _subcategories.Find (s => s.button.activeButton.State == RUIToggleButtonTyped.ButtonState.TRUE);
		}

		// Ajouter une sous catégorie
		internal static PartCategorizer.Category AddSubCategory(string item) {
			System.Random rand = new System.Random ();
			Color _color = new Color (rand.Next (53, 88) / 100f, rand.Next (53, 88) / 100f, rand.Next (53, 88) / 100f);
			EditorPartList.State _displayType = EditorPartList.State.PartsList;
			QPersistent.Add(item, _displayType, _color);
			PartCategorizer.Category _subCategory = AddSubCategory (item, _displayType, _color);
			RUIToggleButtonTyped _btn = FilterPartSearch.button.activeButton;
			if (_btn.State != RUIToggleButtonTyped.ButtonState.FALSE) {
				_btn.SetFalse (_btn, RUIToggleButtonTyped.ClickType.FORCED);
			}
			Refresh ();
			return _subCategory;
		}

		internal static PartCategorizer.Category AddSubCategory(string item, EditorPartList.State displayType, Color color) {
			PartCategorizer.Category _subCategory = PartCategorizer.AddCustomSubcategoryFilter (FilterPartSearch, item, Icon, part => QSearch.FindPart (part));
			_subCategory.displayType = displayType;
			_subCategory.button.activeButton.SetColor (color);
			Quick.Log("Save a research: " + item);
			return _subCategory;
		}

		// Supprimer une sous catégorie
		internal static PartCategorizer.Category DeleteSubCategory(string item) {
			PartCategorizer.Category _subCategory = FilterPartSearch.subcategories.Find (s => s.button.categoryName == item);
			RUIToggleButtonTyped _btn = _subCategory.button.activeButton;
			FilterPartSearch.subcategories.Remove (_subCategory);
			PartCategorizer.Instance.scrollListSub.scrollList.RemoveItem (_subCategory.button.container, true);
			if (_btn.State != RUIToggleButtonTyped.ButtonState.FALSE) {
				_btn.SetFalse (_btn, RUIToggleButtonTyped.ClickType.FORCED);
			}
			Refresh ();
			QPersistent.Remove (item);
			Quick.Log("Delete a research: " + item);
			return _subCategory;
		}

		internal static void Populate() {
			List<string> _items = QPersistent.GetItem;
			foreach (string _item in _items) {
				EditorPartList.State _displayType = (QPersistent.GetDisplayType (_item) == EditorPartList.State.PartsList.ToString () ? EditorPartList.State.PartsList : EditorPartList.State.SubassemblyList);
				AddSubCategory (_item, _displayType, QPersistent.GetColor (_item));
			}
		}

		// Afficher si une sous catégorie existe
		internal static bool Exists(string text) {
			return FilterPartSearch.subcategories.Exists (s => s.button.categoryName == text);
		}

		// Sauvegarder la dernière catégorie
		internal static void SaveLastCategory() {
			SaveLastCategory (CurrentFilter);
		}

		internal static void SaveLastCategory(PartCategorizer.Category Filter) {
			SaveLastCategory (Filter, CurrentSubCategory);
		}

		internal static void SaveLastCategory(PartCategorizer.Category Filter, PartCategorizer.Category SubCategory) {
			if (Filter == FilterPartSearch) {
				return;
			}
			if (Filter != null) {
				PartCategorizer.Category _lastFilter = Filter;
				if (_lastFilter != FilterPartSearch) {
					lastFilter = _lastFilter;
				}
			}
			if (SubCategory != null) {
				PartCategorizer.Category _lastSubCat = SubCategory;
				if (_lastSubCat != SubCategoryPartSearch) {
					lastSubCategory = _lastSubCat;
				}
			}
			lastIsAdvancedMode = EditorLogic.Mode == EditorLogic.EditorModes.ADVANCED;
		}

		// Mettre toutes les sous catégories à faux
		internal static void FlipAllSubCategoryButtons(PartCategorizer.Category category, RUIToggleButtonTyped button, bool setLastState) {
			List<PartCategorizer.Category> _subCategories = category.subcategories;
			foreach (PartCategorizer.Category _subCategory in _subCategories) {
				RUIToggleButtonTyped _btn = _subCategory.button.activeButton;
				if (_btn != button) {
					if (_btn.State != RUIToggleButtonTyped.ButtonState.FALSE) {
						_subCategory.button.LastBtn = setLastState;
						_btn.SetFalse (_btn, RUIToggleButtonTyped.ClickType.FORCED);
					}
				}
			}
		}

		// Mettre une sous catégorie à vrai
		internal static void SubCategorySetTrue(RUIToggleButtonTyped button) {
			if (button.State != RUIToggleButtonTyped.ButtonState.TRUE) {
				button.SetTrue (button, RUIToggleButtonTyped.ClickType.FORCED, false);
			}
			FlipAllSubCategoryButtons (FilterPartSearch, button, true);
		}

		internal static void Refresh() {
			PartCategorizer.Instance.SetAdvancedMode ();
			RUIToggleButtonTyped _btn = FilterPartSearch.button.activeButton;
			if (_btn.State != RUIToggleButtonTyped.ButtonState.TRUE) {
				_btn.SetTrue (_btn, RUIToggleButtonTyped.ClickType.FORCED, false);
			}
			FilterPartSearch.FlipAllFilterButtons (_btn, true);
			FilterPartSearch.FlipAllCategoryButtons (_btn, true);
			if (QSearch.Text == string.Empty) {
				SubCategorySetTrue (SubCategoryPartSearch.button.activeButton);
			} else {
				PartCategorizer.Category _subCategory = FilterPartSearch.subcategories.Find (s => s.button.categoryName == QSearch.Text);
				if (_subCategory != null) {
					SubCategorySetTrue (_subCategory.button.activeButton);
				} else {
					SubCategorySetTrue (SubCategoryPartSearch.button.activeButton);
				}
			}
			EditorPartList.Instance.Refresh ();
		}
	}
}