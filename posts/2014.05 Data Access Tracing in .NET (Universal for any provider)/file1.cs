public abstract class TraceableDbProviderFactory : DbProviderFactory {
    static TraceSource trace = new TraceSource ("TraceableDbProviderFactory");
    public static TraceSource Trace { get { return trace; } }

    static bool isEnabled = false;
    public static bool IsEnabled {
        get { return isEnabled; }
        set {
            if (isEnabled == value) return;
            if (value)
                Register ();
            else
                Unregister ();
            isEnabled = value;
        }
    }

    static TraceableDbProviderFactory () { }

    static void Register () {
        var providers = FindDbProviderFactoryTable ();
        providers.Columns["AssemblyQualifiedName"].ReadOnly = false;

        foreach (DataRow registeredFactory in providers.Rows) {
            var currentType = Type.GetType (registeredFactory["AssemblyQualifiedName"] as string);
            if (currentType.IsSubclassOf (typeof (TraceableDbProviderFactory)))
                continue;
            Type type = typeof (TraceableDbProviderFactory<>).MakeGenericType (new Type[] {
                currentType
            });

            registeredFactory["AssemblyQualifiedName"] = type.AssemblyQualifiedName;
        }

        providers.Columns["AssemblyQualifiedName"].ReadOnly = true;
    }

    static void Unregister () {
        var providers = FindDbProviderFactoryTable ();
        providers.Columns["AssemblyQualifiedName"].ReadOnly = false;

        foreach (DataRow registeredFactory in providers.Rows) {
            var factory = DbProviderFactories.GetFactory (registeredFactory) as TraceableDbProviderFactory;
            if (factory == null) continue;
            var innerType = factory.Inner.GetType ();
            registeredFactory["AssemblyQualifiedName"] = innerType.AssemblyQualifiedName;
        }

        providers.Columns["AssemblyQualifiedName"].ReadOnly = true;
    }

    static DataTable FindDbProviderFactoryTable () {
        DbProviderFactories.GetFactoryClasses ();
        Type typeFromHandle = typeof (DbProviderFactories);
        FieldInfo fieldInfo = typeFromHandle.GetField ("_configTable", BindingFlags.Static | BindingFlags.NonPublic) ?? typeFromHandle.GetField ("_providerTable", BindingFlags.Static | BindingFlags.NonPublic);
        object value = fieldInfo.GetValue (null);
        if (!(value is DataSet)) {
            return (DataTable) value;
        }
        return ((DataSet) value).Tables["DbProviderFactories"];
    }

    protected static DbProviderFactory GetDbProviderInstance (Type type) {
        var fld = type.GetField ("Instance", BindingFlags.Public | BindingFlags.Static);
        return fld.GetValue (null) as DbProviderFactory;
    }

    public DbProviderFactory Inner { get; protected set; }

}

public class TraceableDbProviderFactory<T>
    : TraceableDbProviderFactory, IServiceProvider
where T : DbProviderFactory {

    public static readonly TraceableDbProviderFactory<T> Instance = new TraceableDbProviderFactory<T> ();

    public TraceableDbProviderFactory () {
        Inner = GetDbProviderInstance (typeof (T)) as T;
    }

    public override DbConnection CreateConnection () {
        if (!IsEnabled) return Inner.CreateConnection ();
        return new TraceableConnection (this);
    }

    public object GetService (Type serviceType) {
        if (!IsEnabled) return ((IServiceProvider) Inner).GetService (serviceType);

        if (serviceType == typeof (DbProviderServices))
            return new TraceableDbProviderServices (this);

        var r = Inner as IServiceProvider;
        var innerRes = r.GetService (serviceType);

        return innerRes;
    }

    public override DbCommand CreateCommand () {
        if (!IsEnabled) return Inner.CreateCommand ();
        var res = new TraceableCommand (this);
        return res;
    }
    public override DbParameter CreateParameter () {
        return Inner.CreateParameter ();
    }
    public override DbCommandBuilder CreateCommandBuilder () {
        return Inner.CreateCommandBuilder ();
    }
}

public class TraceableConnection : DbConnection {
    protected override DbProviderFactory DbProviderFactory {
        get {
            return Parent;
        }
    }

    public DbConnection Inner { get; protected set; }
    public TraceableDbProviderFactory Parent { get; protected set; }
    public TraceableConnection (TraceableDbProviderFactory parent, DbConnection inner = null) {
        this.Parent = parent;
        Inner = inner??parent.Inner.CreateConnection ();
    }

    protected override DbTransaction BeginDbTransaction (IsolationLevel isolationLevel) {
        return Inner.BeginTransaction (isolationLevel);
    }

    public override void ChangeDatabase (string databaseName) {
        Inner.ChangeDatabase (databaseName);
    }

    public override void Close () {
        Inner.Close ();
    }

    public override string ConnectionString {
        get {
            return Inner.ConnectionString;
        }
        set {
            Inner.ConnectionString = value;
        }
    }

    protected override DbCommand CreateDbCommand () {
        return new TraceableCommand (this);
    }

    public override string DataSource {
        get { return Inner.DataSource; }
    }

    public override string Database {
        get { return Inner.Database; }
    }

    public override void Open () {
        Inner.Open ();
    }

    public override string ServerVersion {
        get { return Inner.ServerVersion; }
    }

    public override ConnectionState State {
        get { return Inner.State; }
    }
}

public class TraceableDbProviderServices : DbProviderServices {
    public TraceableDbProviderFactory Parent { get; protected set; }

    public DbProviderServices Inner { get; protected set; }

    public TraceableDbProviderServices (TraceableDbProviderFactory parent) {
        this.Parent = parent;
        var innerServiceProvider = Parent.Inner as IServiceProvider;
        this.Inner = innerServiceProvider.GetService (typeof (DbProviderServices))
        as DbProviderServices;
    }

    protected override DbCommandDefinition CreateDbCommandDefinition (DbProviderManifest providerManifest, System.Data.Common.CommandTrees.DbCommandTree commandTree) {
        var innerRes = Inner.CreateCommandDefinition (providerManifest, commandTree);
        var res = new TraceableCommandDefinition (this, innerRes);
        return res;
    }

    protected override DbProviderManifest GetDbProviderManifest (string manifestToken) {
        var res = Inner.GetProviderManifest (manifestToken);
        return res;
    }

    protected override string GetDbProviderManifestToken (DbConnection connection) {
        var res = Inner.GetProviderManifestToken (connection);
        return res;
    }
}

public class TraceableCommandDefinition : DbCommandDefinition {
    public TraceableDbProviderServices Parent { get; protected set; }

    public DbCommandDefinition Inner { get; protected set; }

    public TraceableCommandDefinition (TraceableDbProviderServices parent, DbCommandDefinition inner) {
        this.Parent = parent;
        this.Inner = inner;
    }

    public override DbCommand CreateCommand () {
        var res = new TraceableCommand (Parent.Parent, Inner.CreateCommand ());
        return res;
    }
}

public class TraceableCommand : DbCommand {
    public TraceableDbProviderFactory Parent { get; protected set; }
    public DbCommand Inner { get; protected set; }
    public TraceableCommand (TraceableDbProviderFactory parent, DbCommand inner = null) {
        this.Parent = parent;
        this.Inner = inner?? parent.Inner.CreateCommand ();
    }

    public TraceableCommand (TraceableConnection connection, DbCommand inner = null) {
        this.traceableConnection = connection;
        this.Inner = inner?? connection.Inner.CreateCommand ();
    }

    public override void Cancel () {
        Inner.Cancel ();
    }

    public override string CommandText {
        get {
            return Inner.CommandText;
        }
        set {
            Inner.CommandText = value;
        }
    }

    public override int CommandTimeout {
        get {
            return Inner.CommandTimeout;
        }
        set {
            Inner.CommandTimeout = value;
        }
    }

    public override CommandType CommandType {
        get {
            return Inner.CommandType;
        }
        set {
            Inner.CommandType = value;
        }
    }

    protected override DbParameter CreateDbParameter () {
        return Inner.CreateParameter ();
    }

    TraceableConnection traceableConnection;
    protected override DbConnection DbConnection {
        get {
            return traceableConnection;
        }
        set {
            if (value == null) {
                traceableConnection = null;
                Inner.Connection = null;
                return;
            }
            traceableConnection = value as TraceableConnection;
            if (traceableConnection == null) {
                traceableConnection = new TraceableConnection (Parent, value);
            }
            Inner.Connection = traceableConnection.Inner;
        }
    }

    protected override DbParameterCollection DbParameterCollection {
        get { return Inner.Parameters; }
    }

    protected override DbTransaction DbTransaction {
        get {
            return Inner.Transaction;
        }
        set {
            Inner.Transaction = value;
        }
    }

    public override bool DesignTimeVisible {
        get {
            return Inner.DesignTimeVisible;
        }
        set {
            Inner.DesignTimeVisible = value;
        }
    }

    protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior) {
        if (TraceableDbProviderFactory.IsEnabled)
            Trace.TraceInformation ("DbCommand.ExecuteDataReader \n" + ToString ());
        return Inner.ExecuteReader (behavior);
    }

    public TraceSource Trace { get { return TraceableDbProviderFactory.Trace; } }

    public override string ToString () {
        var sb = new StringBuilder ();
        sb.AppendLine ("===CommandText===");
        sb.AppendLine (CommandText);
        sb.AppendLine ("=================");
        if (Parameters.Count > 0) {
            sb.AppendLine ("===Parameters====");
            foreach (DbParameter parameter in Parameters) {
                sb.AppendLine (string.Format ("{0} = {1}", parameter.ParameterName, parameter.Value));
            }
            sb.AppendLine ("=================");
        }
        //sb.AppendFormat("CommandText=\"{0}\", Parameters={1}"
        //    , CommandText
        //    , string.Join(", ", Parameters.Cast<DbParameter>()
        //    .Select(p => string.Format("{0}:{1}", p.ParameterName, p.Value)))
        //    );
        return sb.ToString ();
    }

    public override int ExecuteNonQuery () {
        if (TraceableDbProviderFactory.IsEnabled)
            Trace.TraceInformation ("DbCommand.ExecuteNonQuery \n" + ToString ());
        return Inner.ExecuteNonQuery ();
    }

    public override object ExecuteScalar () {
        if (TraceableDbProviderFactory.IsEnabled)
            Trace.TraceInformation ("ExecuteScalarCommand: \n" + ToString ());
        return Inner.ExecuteScalar ();
    }

    public override void Prepare () {
        Inner.Prepare ();
    }

    public override UpdateRowSource UpdatedRowSource {
        get {
            return Inner.UpdatedRowSource;
        }
        set {
            Inner.UpdatedRowSource = value;
        }
    }
}