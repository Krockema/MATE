using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.Extensions
{
    public static class ViewHelper
    {
        // Adds an custom entry to Select List First item, state Persistent
        // Usage: selectList.AddFirstItem(item);
        // Returns: new SelectList
        public static SelectList AddFirstItem(this SelectList origList, SelectListItem firstItem)
        {
            List<SelectListItem> newList = origList.ToList();
            newList.Insert(index: 0, item: firstItem);

            var selectedItem = newList.FirstOrDefault(predicate: item => item.Selected);
            var selectedItemValue = String.Empty;
            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(items: newList, dataValueField: "Value", dataTextField: "Text", selectedValue: selectedItemValue);
        }
    }
}
