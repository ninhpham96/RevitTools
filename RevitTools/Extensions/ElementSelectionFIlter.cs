using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTools
{
    
    public static class SelectionExtensions
    {
        public static List<Element> PickElements(this UIDocument uidoc, Func<Element,bool> validateElement,IpickSelectionOption ipickSelectionOption)
        {
            return ipickSelectionOption.PickElements(uidoc, validateElement);
        }
    }

    public interface IpickSelectionOption
    {
        List<Element> PickElements(UIDocument uidoc, Func<Element, bool> validateElement);
    }
    public class CurrentDocumentOption : IpickSelectionOption
    {
        public List<Element> PickElements(UIDocument uidoc, Func<Element, bool> validateElement)
        {
            return uidoc.Selection.PickObjects(ObjectType.Element
                ,SelectionFilterFactory.ElementSelectionFilter(validateElement)).Select(r=> uidoc.Document.GetElement(r.ElementId))
                .ToList();
        }
    }
    public class LinkDocumentOption : IpickSelectionOption
    {
        public List<Element> PickElements(UIDocument uidoc, Func<Element, bool> validateElement)
        {
            var doc = uidoc.Document;
            var references = uidoc.Selection.PickObjects(ObjectType.LinkedElement,SelectionFilterFactory.CreateLinkableSelectionFilter(doc, validateElement));
            var elements = references.Select(r => (doc.GetElement(r.ElementId) as RevitLinkInstance)
            .GetLinkDocument().GetElement(r.LinkedElementId)).ToList();
            return elements;
        }
    }
    public static class SelectionFilterFactory
    {
        public static ElementSelectionFilter ElementSelectionFilter(Func<Element,bool> validateElement)
        {
            return new ElementSelectionFilter(validateElement);
        }
        public static LinkableSelectionFilter CreateLinkableSelectionFilter(Document doc, Func<Element, bool> validateElement)
        {
            return new LinkableSelectionFilter(doc, validateElement);
        }
    }

    public class LinkableSelectionFilter : BaseSelectionFilter
    {
        private readonly Document _doc;
        public LinkableSelectionFilter(Document doc,Func<Element,bool> validateElement) : base(validateElement) 
        {
            _doc = doc;
        }
        public override bool AllowElement(Element elem)
        {
            return true;
        }
        public override bool AllowReference(Reference reference, XYZ position)
        {
            if(!(_doc.GetElement(reference.ElementId)is RevitLinkInstance linkInstance))
                return ValidateElement(_doc.GetElement(reference.ElementId));
            var ele = linkInstance.GetLinkDocument().GetElement(reference.LinkedElementId);
            return ValidateElement(ele);
        }
    }

    public abstract class BaseSelectionFilter : ISelectionFilter
    {
        protected readonly Func<Element, bool> ValidateElement;
        protected BaseSelectionFilter(Func<Element, bool> validateElement)
        {
            ValidateElement = validateElement;
        }
        public abstract bool AllowElement(Element elem);
        public abstract bool AllowReference(Reference reference, XYZ position);
    }

    public class ElementSelectionFilter : BaseSelectionFilter
    {
        private readonly Func<Element, bool> _validateReference;
        public ElementSelectionFilter(Func<Element, bool> validateElement):base(validateElement) { }
        public ElementSelectionFilter(Func<Element, bool> validateElement, Func<Element,bool> validateReference) : base(validateElement)
        {
            _validateReference = validateReference;
        }

        public override bool AllowElement(Element elem)
        {
            return ValidateElement(elem);
        }

        public override bool AllowReference(Reference reference, XYZ position)
        {
            //return _validateReference(reference) ?? true;
            return true;
        }
    }
}
