using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace CPMPayAgentService.LoggingLayer
{

    public class OperationPersister
    {
        string SELECTGENERAL = "SELECT SessionID, ClientAccountID, ContrAgentAccountID, ContrAgentClientAccountID," +
            "ContractBindID, ExternalDocDate, ExternalDocNumber, feeAmount, FunctionType, PaymentDetails," +
            "TransactAmount, CurrentDate, Result, ResultNote, ResultLog, DuetOperCode, DuetOperNote" +
            " FROM Operations ";
        string WHEREID = " WHERE SessionID=@SessionID";
        string WHERERESULT = " WHERE Result=@Result";
        string WHERETIME = " WHERE ExternalDocDate BETWEEN @StartDate AND @EndDate";
        string WHERETIMEANDRESULT = " WHERE Result=@Result AND ExternalDocDate BETWEEN @StartDate AND @EndDate";

        string insertOperationBeginning = "INSERT INTO Operations (";
        string insertOperationMiddle = ") VALUES (";
        string insertOperationEnd = ")";

        string updateOperationBeginning = "UPDATE Operations SET ";
        string updateOperationEnd = " WHERE [SessionID]=@SessionID";

        private static OperationPersister _instance;
        private SqlConnection conn;
        private OperationPersister()
        {
            string connStr = CPMPayAgentService.Properties.Settings.Default.LogsConnectionString;
            conn = new SqlConnection(connStr);
        }

        public static OperationPersister Instance()
        {
            if (_instance == null)
                _instance = new OperationPersister();
            return _instance;
        }

        public Operation getOperation(string sessionID)
        {
            Operation oper = null;
            conn.Open();
            SqlCommand cmd = new SqlCommand(SELECTGENERAL + WHEREID, conn);
            cmd.Parameters.Add("@SessionID", System.Data.SqlDbType.NVarChar, 50/*BigInt*/).Value = sessionID;
            SqlDataReader rdr = cmd.ExecuteReader();//System.Data.CommandBehavior.SingleRow);
            if (rdr.HasRows)
            {
                if (rdr.Read())
                {
                    oper = GetOperationFromReader(rdr);
                }
            }
            rdr.Close();
            conn.Close();
            return oper;
        }

        public void InsertNew(Operation card)
        {
            Operation existingCard = getOperation(card.SessionID);
            if (existingCard != null)
            {
                //throw new NotImplementedException();
                PayAgentLogger.Instance().logError("+++++ Operation " + card.SessionID + " exist. Error. ");
                return;
            }
            StringBuilder columns = new StringBuilder();
            StringBuilder parameters = new StringBuilder();
            conn.Open();
            try
            {

                SqlCommand cmd = new SqlCommand();//cmdStrPermCard, conn);
                cmd.Parameters.Add("@SessionID", System.Data.SqlDbType.NVarChar, 50/*BigInt*/).Value = card.SessionID;
                columns.Append("[SessionID]");
                parameters.Append("@SessionID");

                if (card.ClientAccountID.HasValue)
                {
                    columns.Append(",[ClientAccountID]");
                    parameters.Append("," + "@ClientAccountID");
                    cmd.Parameters.Add("@ClientAccountID", System.Data.SqlDbType.Int).Value = card.ClientAccountID;
                }

                if (card.ContractBindID.HasValue)
                {
                    columns.Append(",[ContractBindID]");
                    parameters.Append("," + "@ContractBindID");
                    cmd.Parameters.Add("@ContractBindID", System.Data.SqlDbType.Int).Value = card.ContractBindID;
                }

                if (card.ContrAgentAccountID.HasValue)
                {
                    columns.Append(",[ContrAgentAccountID]");
                    parameters.Append("," + "@ContrAgentAccountID");
                    cmd.Parameters.Add("@ContrAgentAccountID", System.Data.SqlDbType.Int).Value = card.ContrAgentAccountID;
                }

                if (card.ContrAgentClientAccountID.HasValue)
                {
                    columns.Append(",[ContrAgentClientAccountID]");
                    parameters.Append("," + "@ContrAgentClientAccountID");
                    cmd.Parameters.Add("@ContrAgentClientAccountID", System.Data.SqlDbType.Int).Value = card.ContrAgentClientAccountID;
                }

                if (card.CurrentDate.HasValue)
                {
                    columns.Append(",[CurrentDate]");
                    parameters.Append("," + "@CurrentDate");
                    cmd.Parameters.Add("@CurrentDate", System.Data.SqlDbType.DateTime).Value = card.CurrentDate;
                }

                if (card.DuetOperCode.HasValue)
                {
                    columns.Append("," + "[DuetOperCode]");
                    parameters.Append("," + "@DuetOperCode");
                    cmd.Parameters.Add("@DuetOperCode", System.Data.SqlDbType.Int).Value = card.DuetOperCode;
                }

                if (!string.IsNullOrEmpty(card.DuetOperNote))
                {
                    columns.Append("," + "[DuetOperNote]");
                    parameters.Append("," + "@DuetOperNote");
                    cmd.Parameters.Add("@DuetOperNote", System.Data.SqlDbType.NVarChar, 250).Value = card.DuetOperNote;
                }

                if (card.ExternalDocDate.HasValue)
                {
                    columns.Append(",[ExternalDocDate]");
                    parameters.Append("," + "@ExternalDocDate");
                    cmd.Parameters.Add("@ExternalDocDate", System.Data.SqlDbType.DateTime).Value = card.ExternalDocDate;
                }

                if (!string.IsNullOrEmpty(card.ExternalDocNumber))
                {
                    columns.Append(",[ExternalDocNumber]");
                    parameters.Append("," + "@ExternalDocNumber");
                    cmd.Parameters.Add("@ExternalDocNumber", System.Data.SqlDbType.NVarChar, 50).Value = card.ExternalDocNumber;
                }

                if (card.feeAmount.HasValue)
                {
                    columns.Append(",[feeAmount]");
                    parameters.Append("," + "@feeAmount");
                    cmd.Parameters.Add("@feeAmount", System.Data.SqlDbType.Decimal).Value = card.feeAmount;
                }

                //if (card.Email != null)
                //{
                columns.Append(",[FunctionType]");
                parameters.Append("," + "@FunctionType");
                cmd.Parameters.Add("@FunctionType", System.Data.SqlDbType.NVarChar, 40).Value = card.FunctionType;
                //}

                if (!string.IsNullOrEmpty(card.PaymentDetails))
                {
                    columns.Append(",[PaymentDetails]");
                    parameters.Append("," + "@PaymentDetails");
                    cmd.Parameters.Add("@PaymentDetails", System.Data.SqlDbType.NVarChar, 250).Value = card.PaymentDetails;
                }

                if (card.Result.HasValue)
                {
                    columns.Append(",[Result]");
                    parameters.Append("," + "@Result");
                    cmd.Parameters.Add("@Result", System.Data.SqlDbType.Int).Value = card.Result;
                }

                if (!string.IsNullOrEmpty(card.ResultLog))
                {
                    columns.Append(",[ResultLog]");
                    parameters.Append("," + "@ResultLog");
                    cmd.Parameters.Add("@ResultLog", System.Data.SqlDbType.NVarChar, 250).Value = card.ResultLog;
                }

                if (!string.IsNullOrEmpty(card.ResultNote))
                {
                    columns.Append(",[ResultNote]");
                    parameters.Append("," + "@ResultNote");
                    cmd.Parameters.Add("@ResultNote", System.Data.SqlDbType.NVarChar, 250).Value = card.ResultNote;
                }

                //if (card.State != null)
                //{
                columns.Append(",[TransactAmount]");
                parameters.Append("," + "@TransactAmount");
                cmd.Parameters.Add("@TransactAmount", System.Data.SqlDbType.Decimal).Value = card.TransactAmount;
                //}

                cmd.Connection = conn;
                cmd.CommandText = insertOperationBeginning + columns.ToString() + insertOperationMiddle + parameters.ToString() + insertOperationEnd;

                int rows = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                PayAgentLogger.Instance().logError("Operation was not inserted: " + card.ToString() + " Reason: " + ex.Message);
            }
            finally { conn.Close(); }

        }

        /// <summary>
        /// Update can occur only on 5 fields - Result, ResultLog, ResultNote, DuetoperCode, DuetOperNode
        /// </summary>
        /// <param name="card"></param>
        public void UpdateExisting(Operation card)
        {
            Operation existingCard = getOperation(card.SessionID);
            int rows = 0;
            StringBuilder parameters = new StringBuilder();
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand();

                cmd.Parameters.Add("@SessionID", System.Data.SqlDbType.NVarChar, 50/*BigInt*/).Value = card.SessionID;

                if (card.Result.HasValue)
                {
                    parameters.Append("[Result]=@Result");
                    cmd.Parameters.Add("@Result", System.Data.SqlDbType.Int).Value = card.Result;
                }

                if (!string.IsNullOrEmpty(card.ResultLog))
                {
                    parameters.Append("," + "[ResultLog]=@ResultLog");
                    cmd.Parameters.Add("@ResultLog", System.Data.SqlDbType.NVarChar, 250).Value = card.ResultLog;
                }

                if (!string.IsNullOrEmpty(card.ResultNote))
                {
                    parameters.Append("," + "[ResultNote]=@ResultNote");
                    cmd.Parameters.Add("@ResultNote", System.Data.SqlDbType.NVarChar, 250).Value = card.ResultNote;
                }

                if (!string.IsNullOrEmpty(card.DuetOperNote))
                {
                    parameters.Append("," + "[DuetOperNote]=@DuetOperNote");
                    cmd.Parameters.Add("@DuetOperNote", System.Data.SqlDbType.NVarChar, 250).Value = card.DuetOperNote;
                }

                if (card.DuetOperCode.HasValue)
                {
                    parameters.Append(","+"[DuetOperCode]=@DuetOperCode");
                    cmd.Parameters.Add("@DuetOperCode", System.Data.SqlDbType.Int).Value = card.DuetOperCode;
                }

                if (card.CurrentDate.HasValue)
                {
                    parameters.Append(","+"[CurrentDate]=@CurrentDate");
                    cmd.Parameters.Add("@CurrentDate", System.Data.SqlDbType.DateTime).Value = card.CurrentDate;
                }

                cmd.Connection = conn;
                cmd.CommandText = updateOperationBeginning + parameters.ToString() + updateOperationEnd;

                rows = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //trans.Rollback();
                PayAgentLogger.Instance().logError("error updating the rows in DB (PermCard). The info for PermCard " + card.ToString() + " was not updated. Reason: " + ex.Message);
            }
            finally { conn.Close(); }
        }

        public Operation[] RetrieveOperations(int result)
        {
            List<Operation> opers = new List<Operation>();

            conn.Open();
            SqlCommand cmd = new SqlCommand(SELECTGENERAL + WHERERESULT, conn);
            cmd.Parameters.Add("@Result", System.Data.SqlDbType.Int).Value = result;
            SqlDataReader rdr = cmd.ExecuteReader();//System.Data.CommandBehavior.SingleRow);
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Operation oper = GetOperationFromReader(rdr);
                    opers.Add(oper);
                }
            }
            rdr.Close();
            conn.Close();
            return opers.ToArray();
        }

        public Operation[] RetrieveOperations(int result, DateTime startdate, DateTime enddate)
        {
            List<Operation> opers = new List<Operation>();

            conn.Open();
            SqlCommand cmd = new SqlCommand(SELECTGENERAL + WHERETIMEANDRESULT, conn);
            cmd.Parameters.Add("@Result", System.Data.SqlDbType.Int).Value = result;
            cmd.Parameters.Add("@StartDate", System.Data.SqlDbType.DateTime).Value = startdate;
            cmd.Parameters.Add("@EndDate", System.Data.SqlDbType.DateTime).Value = enddate;
            SqlDataReader rdr = cmd.ExecuteReader();//System.Data.CommandBehavior.SingleRow);
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Operation oper = GetOperationFromReader(rdr);
                    opers.Add(oper);
                }
            }
            rdr.Close();
            conn.Close();
            return opers.ToArray();
        }

        public Operation[] RetrieveOperations(DateTime startdate, DateTime enddate)
        {
            List<Operation> opers = new List<Operation>();

            conn.Open();
            SqlCommand cmd = new SqlCommand(SELECTGENERAL + WHERETIME, conn);
            cmd.Parameters.Add("@StartDate", System.Data.SqlDbType.DateTime).Value = startdate;
            cmd.Parameters.Add("@EndDate", System.Data.SqlDbType.DateTime).Value = enddate;
            SqlDataReader rdr = cmd.ExecuteReader();//System.Data.CommandBehavior.SingleRow);
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Operation oper = GetOperationFromReader(rdr);
                    opers.Add(oper);
                }
            }
            rdr.Close();
            conn.Close();
            return opers.ToArray();
        }

        #region helper methods
        private Operation GetOperationFromReader(SqlDataReader reader)
        {
            Operation card = new Operation();
            string cardID = reader.GetString(0);
            card.SessionID = cardID;

            if (!reader.IsDBNull(1))
            {
                int cardNumber = reader.GetInt32(1);
                card.ClientAccountID = cardNumber;
            }

            if (!reader.IsDBNull(2))
            {
                int cardNumber = reader.GetInt32(2);
                card.ContrAgentAccountID = cardNumber;
            }

            if (!reader.IsDBNull(3))
            {
                int cardNumber = reader.GetInt32(3);
                card.ContrAgentClientAccountID = cardNumber;
            }

            if (!reader.IsDBNull(4))
            {
                int cardNumber = reader.GetInt32(4);
                card.ContractBindID = cardNumber;
            }

            if (!reader.IsDBNull(5))
            {
                DateTime cardNumber = reader.GetDateTime(5);
                card.ExternalDocDate = cardNumber;
            }

            if (!reader.IsDBNull(6))
            {
                string fName = reader.GetString(6);
                card.ExternalDocNumber = fName;
            }

            if (!reader.IsDBNull(7))
            {
                decimal mName = reader.GetDecimal(7);
                card.feeAmount = mName;
            }

            //// not null
            int famName = reader.GetInt32(8);
            card.FunctionType = famName;

            /*SessionID, ClientAccountID, ContrAgentAccountID, ContrAgentClientAccountID,"+
            "ContractBindID, ExternalDocDate, ExternalDocNumber, feeAmount, FunctionType, PaymentDetails,"+
            "TransactAmount, CurrentDate, Result, ResultNote, ResultLog, DuetOperCode, DuetOperNote*/
            if (!reader.IsDBNull(9))
            {
                string title = reader.GetString(9);
                card.PaymentDetails = title;
            }

            ////not null
            decimal bDay = reader.GetDecimal(10);
            card.TransactAmount = bDay;

            if (!reader.IsDBNull(11))
            {
                DateTime cellPhone = reader.GetDateTime(11);
                card.CurrentDate = cellPhone;
            }

            if (!reader.IsDBNull(12))
            {
                int email = reader.GetInt32(12);
                card.Result = email;
            }

            string zipCode = "";
            if (!reader.IsDBNull(13))
            {
                zipCode = reader.GetString(13);
                card.ResultNote = zipCode;
            }

            string street = "";
            if (!reader.IsDBNull(14))
            {
                street = reader.GetString(14);
                card.ResultLog = street;
            }

            if (!reader.IsDBNull(15))
            {
                int city = reader.GetInt32(15);
                card.DuetOperCode = city;
            }

            string state = "";
            if (!reader.IsDBNull(16))
            {
                state = reader.GetString(16);
                card.DuetOperNote = state;
            }
            /*SessionID, ClientAccountID, ContrAgentAccountID, ContrAgentClientAccountID,"+
            "ContractBindID, ExternalDocDate, ExternalDocNumber, feeAmount, FunctionType, PaymentDetails,"+
            "TransactAmount, CurrentDate, Result, ResultNote, ResultLog, DuetOperCode, DuetOperNote*/
            return card;
        }
        #endregion
    }

}
