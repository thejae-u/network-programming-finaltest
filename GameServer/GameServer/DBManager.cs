using System;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace GameServer
{
    internal class DBManager(string conn)
    {
        private MySqlConnection _conn = new(conn);

        public bool Login(string uid, string pw)
        {
            _conn.Open();
            var query = "SELECT * FROM userinfo WHERE uid = @userID";
            try
            {
                MySqlCommand cmd = new(query, _conn);
                cmd.Parameters.AddWithValue("@userID", uid);
                MySqlDataReader rd = cmd.ExecuteReader();

                if (rd.HasRows)
                {
                    rd.Close();
                    query = "SELECT uid FROM userinfo WHERE pw = SHA2(@userPW, 256)";
                    cmd = new(query, _conn);
                    cmd.Parameters.AddWithValue("@userPW", pw);
                    rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        if (rd.GetString("uid") == uid)
                        {
                            rd.Close();
                            return true;
                        }
                    }
                }

                rd.Close();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool SignUp(string uid, string pw)
        {
            try
            {
                _conn.Open();
                var query = "Insert INTO userinfo VALUES(@userId, sha2(@userPW, 256), 0)";
                MySqlCommand cmd = new(query, _conn);
                cmd.Parameters.AddWithValue("@userID", uid);
                cmd.Parameters.AddWithValue("@userPW", pw);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool Goal(string userId, float score)
        {
            var query = "SELECT score FROM userinfo WHERE uid = @userID";
            try
            {
                _conn.Open();
                MySqlCommand cmd = new(query);
                cmd.Parameters.AddWithValue("@userID", userId);
                MySqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    float savedTime = float.Parse(rd.GetString("score"));
                    if(score > savedTime)
                    {
                        rd.Close();
                        query = "UPDATE userinfo SET score = @newScore WHERE uid = @userID";
                        cmd = new(query, _conn);
                        cmd.Parameters.AddWithValue("@newScore", score);
                        cmd.Parameters.AddWithValue("@userID", userId);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public byte[]? Best(string userId)
        {
            var query = "SELECT score FROM userinfo WHERE uid = @userID";
            try
            {
                _conn.Open();
                MySqlCommand cmd = new(query, _conn);
                cmd.Parameters.AddWithValue("@userID", userId);
                MySqlDataReader rd = cmd.ExecuteReader();
                while(rd.Read())
                {
                    return Encoding.UTF8.GetBytes(rd.GetString("score"));
                }

                return null;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                _conn.Close();
            }
        }
    }
}
