﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.34209
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by Microsoft.Silverlight.ServiceReference, version 5.0.61118.0
// 
namespace Lite.ServiceAlmacenaCliente {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.grr.net.fv/svc", ConfigurationName="ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType")]
    public interface WsAlmacenaUbicacionClienteFVPortType {
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="urn:almacenaUbicacionClienteFV", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.IAsyncResult BeginalmacenaUbicacionClienteFV(Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest request, System.AsyncCallback callback, object asyncState);
        
        Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse EndalmacenaUbicacionClienteFV(System.IAsyncResult result);
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName="wsAlmacenaUbicacionClienteFV-RQ-Type", Namespace="http://www.grr.net.fv/gis")]
    public partial class wsAlmacenaUbicacionClienteFVRQType : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string numero_cuentaField;
        
        private string etiquetaField;
        
        private string tipoField;
        
        private string estadoField;
        
        private string originadorField;
        
        private string longitudField;
        
        private string latitudField;
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=0)]
        public string numero_cuenta {
            get {
                return this.numero_cuentaField;
            }
            set {
                this.numero_cuentaField = value;
                this.RaisePropertyChanged("numero_cuenta");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=1)]
        public string etiqueta {
            get {
                return this.etiquetaField;
            }
            set {
                this.etiquetaField = value;
                this.RaisePropertyChanged("etiqueta");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=2)]
        public string tipo {
            get {
                return this.tipoField;
            }
            set {
                this.tipoField = value;
                this.RaisePropertyChanged("tipo");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=3)]
        public string estado {
            get {
                return this.estadoField;
            }
            set {
                this.estadoField = value;
                this.RaisePropertyChanged("estado");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=4)]
        public string originador {
            get {
                return this.originadorField;
            }
            set {
                this.originadorField = value;
                this.RaisePropertyChanged("originador");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=5)]
        public string longitud {
            get {
                return this.longitudField;
            }
            set {
                this.longitudField = value;
                this.RaisePropertyChanged("longitud");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true, Order=6)]
        public string latitud {
            get {
                return this.latitudField;
            }
            set {
                this.latitudField = value;
                this.RaisePropertyChanged("latitud");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.grr.net.fv/gis")]
    public partial class detallerespuestatype : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string fechaRespuestaField;
        
        private string codigoRespuestaField;
        
        private string codigoErrorField;
        
        private string descripcionErrorField;
        
        private string mensajeErrorField;
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string FechaRespuesta {
            get {
                return this.fechaRespuestaField;
            }
            set {
                this.fechaRespuestaField = value;
                this.RaisePropertyChanged("FechaRespuesta");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string CodigoRespuesta {
            get {
                return this.codigoRespuestaField;
            }
            set {
                this.codigoRespuestaField = value;
                this.RaisePropertyChanged("CodigoRespuesta");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string CodigoError {
            get {
                return this.codigoErrorField;
            }
            set {
                this.codigoErrorField = value;
                this.RaisePropertyChanged("CodigoError");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string DescripcionError {
            get {
                return this.descripcionErrorField;
            }
            set {
                this.descripcionErrorField = value;
                this.RaisePropertyChanged("DescripcionError");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public string MensajeError {
            get {
                return this.mensajeErrorField;
            }
            set {
                this.mensajeErrorField = value;
                this.RaisePropertyChanged("MensajeError");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.grr.net.fv/gis")]
    public partial class InfoCliente : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string idSwField;
        
        private string estadoField;
        
        private detallerespuestatype detalle_RespuestaField;
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string idSw {
            get {
                return this.idSwField;
            }
            set {
                this.idSwField = value;
                this.RaisePropertyChanged("idSw");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string estado {
            get {
                return this.estadoField;
            }
            set {
                this.estadoField = value;
                this.RaisePropertyChanged("estado");
            }
        }
        
        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public detallerespuestatype Detalle_Respuesta {
            get {
                return this.detalle_RespuestaField;
            }
            set {
                this.detalle_RespuestaField = value;
                this.RaisePropertyChanged("Detalle_Respuesta");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class almacenaUbicacionClienteFVRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="wsAlmacenaUbicacionClienteFV-RQ", Namespace="http://www.grr.net.fv/gis", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute("wsAlmacenaUbicacionClienteFV-RQ")]
        public Lite.ServiceAlmacenaCliente.wsAlmacenaUbicacionClienteFVRQType wsAlmacenaUbicacionClienteFVRQ;
        
        public almacenaUbicacionClienteFVRequest() {
        }
        
        public almacenaUbicacionClienteFVRequest(Lite.ServiceAlmacenaCliente.wsAlmacenaUbicacionClienteFVRQType wsAlmacenaUbicacionClienteFVRQ) {
            this.wsAlmacenaUbicacionClienteFVRQ = wsAlmacenaUbicacionClienteFVRQ;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class almacenaUbicacionClienteFVResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="wsAlmacenaUbicacionClienteFV-RS", Namespace="http://www.grr.net.fv/gis", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute("wsAlmacenaUbicacionClienteFV-RS")]
        public Lite.ServiceAlmacenaCliente.InfoCliente wsAlmacenaUbicacionClienteFVRS;
        
        public almacenaUbicacionClienteFVResponse() {
        }
        
        public almacenaUbicacionClienteFVResponse(Lite.ServiceAlmacenaCliente.InfoCliente wsAlmacenaUbicacionClienteFVRS) {
            this.wsAlmacenaUbicacionClienteFVRS = wsAlmacenaUbicacionClienteFVRS;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface WsAlmacenaUbicacionClienteFVPortTypeChannel : Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class almacenaUbicacionClienteFVCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public almacenaUbicacionClienteFVCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WsAlmacenaUbicacionClienteFVPortTypeClient : System.ServiceModel.ClientBase<Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType>, Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType {
        
        private BeginOperationDelegate onBeginalmacenaUbicacionClienteFVDelegate;
        
        private EndOperationDelegate onEndalmacenaUbicacionClienteFVDelegate;
        
        private System.Threading.SendOrPostCallback onalmacenaUbicacionClienteFVCompletedDelegate;
        
        private BeginOperationDelegate onBeginOpenDelegate;
        
        private EndOperationDelegate onEndOpenDelegate;
        
        private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
        
        private BeginOperationDelegate onBeginCloseDelegate;
        
        private EndOperationDelegate onEndCloseDelegate;
        
        private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
        
        public WsAlmacenaUbicacionClienteFVPortTypeClient() {
        }
        
        public WsAlmacenaUbicacionClienteFVPortTypeClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WsAlmacenaUbicacionClienteFVPortTypeClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WsAlmacenaUbicacionClienteFVPortTypeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WsAlmacenaUbicacionClienteFVPortTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Net.CookieContainer CookieContainer {
            get {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    return httpCookieContainerManager.CookieContainer;
                }
                else {
                    return null;
                }
            }
            set {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else {
                    throw new System.InvalidOperationException("No se puede establecer el objeto CookieContainer. Asegúrese de que el enlace cont" +
                            "iene un objeto HttpCookieContainerBindingElement.");
                }
            }
        }
        
        public event System.EventHandler<almacenaUbicacionClienteFVCompletedEventArgs> almacenaUbicacionClienteFVCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType.BeginalmacenaUbicacionClienteFV(Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest request, System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginalmacenaUbicacionClienteFV(request, callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType.EndalmacenaUbicacionClienteFV(System.IAsyncResult result) {
            return base.Channel.EndalmacenaUbicacionClienteFV(result);
        }
        
        private System.IAsyncResult OnBeginalmacenaUbicacionClienteFV(object[] inValues, System.AsyncCallback callback, object asyncState) {
            Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest request = ((Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest)(inValues[0]));
            return ((Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType)(this)).BeginalmacenaUbicacionClienteFV(request, callback, asyncState);
        }
        
        private object[] OnEndalmacenaUbicacionClienteFV(System.IAsyncResult result) {
            Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse retVal = ((Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType)(this)).EndalmacenaUbicacionClienteFV(result);
            return new object[] {
                    retVal};
        }
        
        private void OnalmacenaUbicacionClienteFVCompleted(object state) {
            if ((this.almacenaUbicacionClienteFVCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.almacenaUbicacionClienteFVCompleted(this, new almacenaUbicacionClienteFVCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void almacenaUbicacionClienteFVAsync(Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest request) {
            this.almacenaUbicacionClienteFVAsync(request, null);
        }
        
        public void almacenaUbicacionClienteFVAsync(Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest request, object userState) {
            if ((this.onBeginalmacenaUbicacionClienteFVDelegate == null)) {
                this.onBeginalmacenaUbicacionClienteFVDelegate = new BeginOperationDelegate(this.OnBeginalmacenaUbicacionClienteFV);
            }
            if ((this.onEndalmacenaUbicacionClienteFVDelegate == null)) {
                this.onEndalmacenaUbicacionClienteFVDelegate = new EndOperationDelegate(this.OnEndalmacenaUbicacionClienteFV);
            }
            if ((this.onalmacenaUbicacionClienteFVCompletedDelegate == null)) {
                this.onalmacenaUbicacionClienteFVCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnalmacenaUbicacionClienteFVCompleted);
            }
            base.InvokeAsync(this.onBeginalmacenaUbicacionClienteFVDelegate, new object[] {
                        request}, this.onEndalmacenaUbicacionClienteFVDelegate, this.onalmacenaUbicacionClienteFVCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
        
        private object[] OnEndOpen(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
        
        private void OnOpenCompleted(object state) {
            if ((this.OpenCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OpenAsync() {
            this.OpenAsync(null);
        }
        
        public void OpenAsync(object userState) {
            if ((this.onBeginOpenDelegate == null)) {
                this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
            }
            if ((this.onEndOpenDelegate == null)) {
                this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
            }
            if ((this.onOpenCompletedDelegate == null)) {
                this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
            }
            base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
        
        private object[] OnEndClose(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
            return null;
        }
        
        private void OnCloseCompleted(object state) {
            if ((this.CloseCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void CloseAsync() {
            this.CloseAsync(null);
        }
        
        public void CloseAsync(object userState) {
            if ((this.onBeginCloseDelegate == null)) {
                this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
            }
            if ((this.onEndCloseDelegate == null)) {
                this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
            }
            if ((this.onCloseCompletedDelegate == null)) {
                this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
            }
            base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
        }
        
        protected override Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType CreateChannel() {
            return new WsAlmacenaUbicacionClienteFVPortTypeClientChannel(this);
        }
        
        private class WsAlmacenaUbicacionClienteFVPortTypeClientChannel : ChannelBase<Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType>, Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType {
            
            public WsAlmacenaUbicacionClienteFVPortTypeClientChannel(System.ServiceModel.ClientBase<Lite.ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortType> client) : 
                    base(client) {
            }
            
            public System.IAsyncResult BeginalmacenaUbicacionClienteFV(Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest request, System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[1];
                _args[0] = request;
                System.IAsyncResult _result = base.BeginInvoke("almacenaUbicacionClienteFV", _args, callback, asyncState);
                return _result;
            }
            
            public Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse EndalmacenaUbicacionClienteFV(System.IAsyncResult result) {
                object[] _args = new object[0];
                Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse _result = ((Lite.ServiceAlmacenaCliente.almacenaUbicacionClienteFVResponse)(base.EndInvoke("almacenaUbicacionClienteFV", _args, result)));
                return _result;
            }
        }
    }
}
