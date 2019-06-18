using System;
using System.Data;
using log4net.Core;
using log4net.Layout;

namespace SeqAppender.Log4net.Core
{
  /// <summary>
  /// Parameter type used by the <see cref="T:log4net.Appender.AdoNetAppender" />.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class provides the basic database parameter properties
  /// as defined by the <see cref="T:System.Data.IDbDataParameter" /> interface.
  /// </para>
  /// <para>This type can be subclassed to provide database specific
  /// functionality. The two methods that are called externally are
  /// <see cref="M:log4net.Appender.AdoNetAppenderParameter.Prepare(System.Data.IDbCommand)" /> and <see cref="M:log4net.Appender.AdoNetAppenderParameter.FormatValue(System.Data.IDbCommand,log4net.Core.LoggingEvent)" />.
  /// </para>
  /// </remarks>
  public class AdoNetAppenderParameter
  {
    /// <summary>Flag to infer type rather than use the DbType</summary>
    private bool m_inferType = true;

    /// <summary>The name of this parameter.</summary>
    private string m_parameterName;

    /// <summary>The database type for this parameter.</summary>
    private DbType m_dbType;

    /// <summary>The precision for this parameter.</summary>
    private byte m_precision;

    /// <summary>The scale for this parameter.</summary>
    private byte m_scale;

    /// <summary>The size for this parameter.</summary>
    private int m_size;

    /// <summary>
    /// The <see cref="T:log4net.Layout.IRawLayout" /> to use to render the
    /// logging event into an object for this parameter.
    /// </summary>
    private IRawLayout m_layout;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:log4net.Appender.AdoNetAppenderParameter" /> class.
    /// </summary>
    /// <remarks>
    /// Default constructor for the AdoNetAppenderParameter class.
    /// </remarks>
    public AdoNetAppenderParameter()
    {
      this.Precision = (byte) 0;
      this.Scale = (byte) 0;
      this.Size = 0;
    }

    /// <summary>Gets or sets the name of this parameter.</summary>
    /// <value>The name of this parameter.</value>
    /// <remarks>
    /// <para>
    /// The name of this parameter. The parameter name
    /// must match up to a named parameter to the SQL stored procedure
    /// or prepared statement.
    /// </para>
    /// </remarks>
    public string ParameterName
    {
      get { return this.m_parameterName; }
      set { this.m_parameterName = value; }
    }

    /// <summary>Gets or sets the database type for this parameter.</summary>
    /// <value>The database type for this parameter.</value>
    /// <remarks>
    /// <para>
    /// The database type for this parameter. This property should
    /// be set to the database type from the <see cref="P:log4net.Appender.AdoNetAppenderParameter.DbType" />
    /// enumeration. See <see cref="P:System.Data.IDataParameter.DbType" />.
    /// </para>
    /// <para>
    /// This property is optional. If not specified the ADO.NET provider
    /// will attempt to infer the type from the value.
    /// </para>
    /// </remarks>
    /// <seealso cref="P:System.Data.IDataParameter.DbType" />
    public DbType DbType
    {
      get { return this.m_dbType; }
      set
      {
        this.m_dbType = value;
        this.m_inferType = false;
      }
    }

    /// <summary>Gets or sets the precision for this parameter.</summary>
    /// <value>The precision for this parameter.</value>
    /// <remarks>
    /// <para>
    /// The maximum number of digits used to represent the Value.
    /// </para>
    /// <para>
    /// This property is optional. If not specified the ADO.NET provider
    /// will attempt to infer the precision from the value.
    /// </para>
    /// </remarks>
    /// <seealso cref="P:System.Data.IDbDataParameter.Precision" />
    public byte Precision
    {
      get { return this.m_precision; }
      set { this.m_precision = value; }
    }

    /// <summary>Gets or sets the scale for this parameter.</summary>
    /// <value>The scale for this parameter.</value>
    /// <remarks>
    /// <para>
    /// The number of decimal places to which Value is resolved.
    /// </para>
    /// <para>
    /// This property is optional. If not specified the ADO.NET provider
    /// will attempt to infer the scale from the value.
    /// </para>
    /// </remarks>
    /// <seealso cref="P:System.Data.IDbDataParameter.Scale" />
    public byte Scale
    {
      get { return this.m_scale; }
      set { this.m_scale = value; }
    }

    /// <summary>Gets or sets the size for this parameter.</summary>
    /// <value>The size for this parameter.</value>
    /// <remarks>
    /// <para>
    /// The maximum size, in bytes, of the data within the column.
    /// </para>
    /// <para>
    /// This property is optional. If not specified the ADO.NET provider
    /// will attempt to infer the size from the value.
    /// </para>
    /// <para>
    /// For BLOB data types like VARCHAR(max) it may be impossible to infer the value automatically, use -1 as the size in this case.
    /// </para>
    /// </remarks>
    /// <seealso cref="P:System.Data.IDbDataParameter.Size" />
    public int Size
    {
      get { return this.m_size; }
      set { this.m_size = value; }
    }

    /// <summary>
    /// Gets or sets the <see cref="T:log4net.Layout.IRawLayout" /> to use to
    /// render the logging event into an object for this
    /// parameter.
    /// </summary>
    /// <value>
    /// The <see cref="T:log4net.Layout.IRawLayout" /> used to render the
    /// logging event into an object for this parameter.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="T:log4net.Layout.IRawLayout" /> that renders the value for this
    /// parameter.
    /// </para>
    /// <para>
    /// The <see cref="T:log4net.Layout.RawLayoutConverter" /> can be used to adapt
    /// any <see cref="T:log4net.Layout.ILayout" /> into a <see cref="T:log4net.Layout.IRawLayout" />
    /// for use in the property.
    /// </para>
    /// </remarks>
    public IRawLayout Layout
    {
      get { return this.m_layout; }
      set { this.m_layout = value; }
    }

    /// <summary>Prepare the specified database command object.</summary>
    /// <param name="command">The command to prepare.</param>
    /// <remarks>
    /// <para>
    /// Prepares the database command object by adding
    /// this parameter to its collection of parameters.
    /// </para>
    /// </remarks>
    public virtual void Prepare(IDbCommand command)
    {
      IDbDataParameter parameter = command.CreateParameter();
      parameter.ParameterName = this.ParameterName;
      if (!this.m_inferType)
        parameter.DbType = this.DbType;
      if (this.Precision != (byte) 0)
        parameter.Precision = this.Precision;
      if (this.Scale != (byte) 0)
        parameter.Scale = this.Scale;
      if (this.Size != 0)
        parameter.Size = this.Size;
      command.Parameters.Add((object) parameter);
    }

    /// <summary>
    /// Renders the logging event and set the parameter value in the command.
    /// </summary>
    /// <param name="command">The command containing the parameter.</param>
    /// <param name="loggingEvent">The event to be rendered.</param>
    /// <remarks>
    /// <para>
    /// Renders the logging event using this parameters layout
    /// object. Sets the value of the parameter on the command object.
    /// </para>
    /// </remarks>
    public virtual void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
    {
      ((IDataParameter) command.Parameters[this.ParameterName]).Value = this.Layout.Format(loggingEvent) ?? (object) DBNull.Value;
    }
  }
}