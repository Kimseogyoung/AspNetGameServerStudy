using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassGenerator
{
    public class DatabaseChangeLogData
    {
        public List<DatabaseChangeLog> DatabaseChangeLog { get; set; } = new();
    }

    public class DatabaseChangeLog
    {
        public List<PreConditions> PreConditions { get; set; }
        public ChangeSet ChangeSet { get; set; }
    }

    public class PreConditions
    {
        public RunningAs RunningAs { get; set; } = new();
    }

    public class RunningAs
    {
        public string Username { get; set; }
    }

    public class ChangeSet
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public List<Change> Changes { get; set; } = new();
    }

    public class Change
    {
        public CreateTable CreateTable { get; set; }
        public AddPrimaryKey AddPrimaryKey { get; set; }
        public AddForeignKeyConstraint AddForeignKeyConstraint { get; set; }
        public AddAutoIncrement AddAutoIncrement { get; set; }
        public AddColumn AddColumn { get; set; }
        public CreateIndex CreateIndex { get; set; }
    }

    public class CreateTable
    {
        public string TableName { get; set; }
        public List<Columns> Columns { get; set; } = new();
    }

    public class AddColumn
    {
        public string TableName { get; set; }
        public List<Columns> Columns { get; set; } = new();
    }

    public class AddPrimaryKey
    {
        public string ColumnNames { get; set; } // Key1, Key2
        public string TableName { get; set; }
    }

    public class CreateIndex
    {
        public string IndexName { get; set; }
        public string TableName { get; set; }
        public List<Columns> Columns { get; set; } = new();
    }

    public class AddAutoIncrement
    {
        public string TableName { get; set; }
        public string ColumnDataType { get; set; }
        public string ColumnName { get; set; }
        public bool DefaultOnNull { get; set; } = false;
        public string GenerationType { get; set; } = "ALWAYS";
        public int StartWith { get; set; } = 1;
    }

    public class AddForeignKeyConstraint
    {
        public string BaseTableName { get; set; }
        public string BaseColumnNames { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnNames { get; set; }
        public string ConstraintName { get; set; }
    }

    public class IndexColumn
    {
        public string Name { get; set; }
    }

    public class Columns
    {
        public Column Column { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public ColumnConstraints Constraints { get; set; }
    }

    public class ColumnConstraints
    {
        public bool PrimaryKey { get; set; }
        public bool Nullable { get; set; }
    }
}
