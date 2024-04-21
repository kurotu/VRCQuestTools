using System.Collections.Generic;
using System.IO;
using nadena.dev.ndmf;
#if VQT_HAS_NDMF_ERROR_REPORT
using nadena.dev.ndmf.localization;
using UnityEngine;
#else
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Error for object conversion.
    /// </summary>
    internal class ObjectConversionError : SimpleError
    {
        private readonly System.Type objectType;
        private readonly string objectName;
        private readonly System.Exception exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectConversionError"/> class.
        /// </summary>
        /// <param name="objectReference">Reference for converting object.</param>
        /// <param name="exception">Throwed exception.</param>
        public ObjectConversionError(ObjectReference objectReference, System.Exception exception)
        {
            objectType = objectReference.Object.GetType();
            objectName = objectReference.Object.name;
            this.exception = exception;
            _references = new List<ObjectReference> { objectReference };
        }

        /// <inheritdoc/>
        public override ErrorSeverity Severity => ErrorSeverity.Error;

        /// <inheritdoc/>
        public override Localizer Localizer => NdmfLocalizer.Instance;

        /// <inheritdoc/>
        public override string TitleKey => NdmfLocalizer.ObjectConversionErrorTitle;

        /// <inheritdoc/>
        public override string DetailsKey => NdmfLocalizer.ObjectConversionErrorDescription;

        /// <inheritdoc/>
        public override string[] DetailsSubst => new[] { objectType.Name, objectName, exception.GetType().Name, exception.Message, StackTrace };

        private string StackTrace => exception.StackTrace.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, string.Empty);
    }
}
