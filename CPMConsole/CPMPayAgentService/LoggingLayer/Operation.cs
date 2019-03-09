using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPMPayAgentService.LoggingLayer
{
    public class Operation
    {

        private string _SessionID;

        private System.Nullable<int> _ClientAccountID;

        private System.Nullable<int> _ContrAgentAccountID;

        private System.Nullable<int> _ContrAgentClientAccountID;

        private System.Nullable<int> _ContractBindID;

        private System.Nullable<System.DateTime> _ExternalDocDate;

        private string _ExternalDocNumber;

        private System.Nullable<decimal> _feeAmount;

        private int _FunctionType;

        private string _PaymentDetails;

        private decimal _TransactAmount;

        private System.Nullable<System.DateTime> _CurrentDate;

        private System.Nullable<int> _Result;

        private string _ResultNote;

        private string _ResultLog;

        private System.Nullable<int> _DuetOperCode;

        private string _DuetOperNote;

        public Operation()
        {

        }

        public string SessionID
        {
            get
            {
                return this._SessionID;
            }
            set
            {
                if ((this._SessionID != value))
                {
                    this._SessionID = value;

                }
            }
        }

        public System.Nullable<int> ClientAccountID
        {
            get
            {
                return this._ClientAccountID;
            }
            set
            {
                if ((this._ClientAccountID != value))
                {
                    this._ClientAccountID = value;
                }
            }
        }

        public System.Nullable<int> ContrAgentAccountID
        {
            get
            {
                return this._ContrAgentAccountID;
            }
            set
            {
                if ((this._ContrAgentAccountID != value))
                {
                    this._ContrAgentAccountID = value;
                }
            }
        }

        public System.Nullable<int> ContrAgentClientAccountID
        {
            get
            {
                return this._ContrAgentClientAccountID;
            }
            set
            {
                if ((this._ContrAgentClientAccountID != value))
                {
                    this._ContrAgentClientAccountID = value;
                }
            }
        }

        public System.Nullable<int> ContractBindID
        {
            get
            {
                return this._ContractBindID;
            }
            set
            {
                if ((this._ContractBindID != value))
                {
                    this._ContractBindID = value;
                }
            }
        }

        public System.Nullable<System.DateTime> ExternalDocDate
        {
            get
            {
                return this._ExternalDocDate;
            }
            set
            {
                if ((this._ExternalDocDate != value))
                {
                    this._ExternalDocDate = value;
                }
            }
        }

        public string ExternalDocNumber
        {
            get
            {
                return this._ExternalDocNumber;
            }
            set
            {
                if ((this._ExternalDocNumber != value))
                {
                    this._ExternalDocNumber = value;
                }
            }
        }

        public System.Nullable<decimal> feeAmount
        {
            get
            {
                return this._feeAmount;
            }
            set
            {
                if ((this._feeAmount != value))
                {
                    this._feeAmount = value;
                }
            }
        }

        public int FunctionType
        {
            get
            {
                return this._FunctionType;
            }
            set
            {
                if ((this._FunctionType != value))
                {
                    this._FunctionType = value;
                }
            }
        }


        public string PaymentDetails
        {
            get
            {
                return this._PaymentDetails;
            }
            set
            {
                if ((this._PaymentDetails != value))
                {
                    this._PaymentDetails = value;
                }
            }
        }

        public decimal TransactAmount
        {
            get
            {
                return this._TransactAmount;
            }
            set
            {
                if ((this._TransactAmount != value))
                {
                    this._TransactAmount = value;
                }
            }
        }

        public System.Nullable<System.DateTime> CurrentDate
        {
            get
            {
                return this._CurrentDate;
            }
            set
            {
                if ((this._CurrentDate != value))
                {
                    this._CurrentDate = value;
                }
            }
        }

        public System.Nullable<int> Result
        {
            get
            {
                return this._Result;
            }
            set
            {
                if ((this._Result != value))
                {
                    this._Result = value;
                }
            }
        }

        public string ResultNote
        {
            get
            {
                return this._ResultNote;
            }
            set
            {
                if ((this._ResultNote != value))
                {
                    this._ResultNote = value;
                }
            }
        }

        public string ResultLog
        {
            get
            {
                return this._ResultLog;
            }
            set
            {
                if ((this._ResultLog != value))
                {
                    this._ResultLog = value;
                }
            }
        }

        public System.Nullable<int> DuetOperCode
        {
            get
            {
                return this._DuetOperCode;
            }
            set
            {
                if ((this._DuetOperCode != value))
                {
                    this._DuetOperCode = value;
                }
            }
        }

        public string DuetOperNote
        {
            get
            {
                return this._DuetOperNote;
            }
            set
            {
                if ((this._DuetOperNote != value))
                {
                    this._DuetOperNote = value;
                }
            }
        }
    }
}
