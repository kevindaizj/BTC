﻿using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.RPC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using USDTWallet.Models.Models.Transfer;
using USDTWallet.Models.Models.USDT;

namespace USDTWallet.Common.Operators
{
    public class USDTOperator
    {
        private static Lazy<USDTOperator> _instance = new Lazy<USDTOperator>(() => new USDTOperator());
        public static USDTOperator Instance
        {
            get { return _instance.Value; }
        }

        private RPCClient Client { get; set; }

        public static readonly UInt64 PropertyId = 2147483651;

        private USDTOperator()
        {
            this.ChangeNetwork(NetworkOperator.Instance.Credential,
                               NetworkOperator.Instance.RpcUri,
                               NetworkOperator.Instance.Network);
        }

        public void ChangeNetwork(NetworkCredential credential, Uri rpcUri, Network network)
        {
            this.Client = new RPCClient(credential, rpcUri, network);
        }

        public async Task<Money> GetBalanceByAddress(string address)
        {
            var resp = await Client.SendCommandAsync("omni_getbalance", address, PropertyId);
            var result = resp.Result.ToObject<BalanceResult>();
            return Money.Parse(result.balance);
        }

        public async Task<string> CreatePayloadSimpleSend(Money amount)
        {
            var resp = await Client.SendCommandAsync("omni_createpayload_simplesend", PropertyId, amount.ToString());
            return resp.Result.ToString();
        }

        public async Task<string> GenerateOpRetrun(string transactionHex, string payloadHex)
        {
            var resp = await Client.SendCommandAsync("omni_createrawtx_opreturn", transactionHex, payloadHex);
            return resp.Result.ToString();
        }

        public async Task<string> GenerateReference(string opreturn, string toAddress)
        {
            var resp = await Client.SendCommandAsync("omni_createrawtx_reference", opreturn, toAddress);
            return resp.Result.ToString();
        }

        public async Task<string> CreateRawTxChange(string rawtx, List<Coin> coins, BitcoinAddress destination, Money fee, int position = 0)
        {
            var prevtxs = new JArray();
            foreach (var prev in coins)
            {
                var amount = prev.Amount.ToDecimal(MoneyUnit.BTC);

                JObject prevObj = new JObject();
                prevObj.Add(new JProperty("txid", prev.Outpoint.Hash.ToString()));
                prevObj.Add(new JProperty("vout", prev.Outpoint.N));
                prevObj.Add(new JProperty("scriptPubKey", prev.ScriptPubKey.ToHex()));
                prevObj.Add(new JProperty("value", amount));
                prevtxs.Add(prevObj);
            }


            var resp = await Client.SendCommandAsync("omni_createrawtx_change",
                                          rawtx,
                                          prevtxs,
                                          destination.ToString(),
                                          fee.ToString(),
                                          position);

            return resp.Result.ToString();
        }



        public async Task<Transaction> BuildUnsignedTx(string fromAddress, string toAddress, string changeAddress, Money amount, FeeRate estimateFeeRate, List<Coin> spentCoins)
        {
            var network = NetworkOperator.Instance.Network;
            var to = BitcoinAddress.Create(toAddress, network);
            var change = BitcoinAddress.Create(changeAddress, network);

            var builder = network.CreateTransactionBuilder();
            var tx = builder.AddCoins(spentCoins)
                            .SetChange(change)
                            .SendEstimatedFees(estimateFeeRate)
                            .BuildTransaction(false);

            var detail = tx.ToString();
            
            var amountPayload = await this.CreatePayloadSimpleSend(amount);
            var opreturn = await this.GenerateOpRetrun(tx.ToHex(), amountPayload);
            var receiveRef = await this.GenerateReference(opreturn, toAddress);

            var finalTx = Transaction.Parse(receiveRef, network);

            return finalTx;
        }


        public USDTOpReturnInfo ConvertOpReturnTxOut(TxOut opReturnOutput)
        {
            var script = opReturnOutput.ScriptPubKey.ToString();
            var opcode = "OP_RETURN";
            var omniAscii = "6f6d6e69";
            var starter = opcode + " " + omniAscii;

            if (!script.StartsWith(starter))
                return null;

            var code = script.Substring(starter.Length);
            int index = 0;

            var version = code.Substring(index, 4);
            index += 4;

            var type = code.Substring(index, 4);
            index += 4;

            var currencyId = code.Substring(index, 8);
            index += 8;

            var amount = code.Substring(index);
            var amountDec = UInt64.Parse(amount, NumberStyles.HexNumber);

            return new USDTOpReturnInfo
            {
                TransactionVersion = uint.Parse(version, NumberStyles.HexNumber),
                TransactionType = uint.Parse(type, NumberStyles.HexNumber),
                CurrencyIdentifier = uint.Parse(currencyId, NumberStyles.HexNumber),
                Amount = new Money(amountDec, MoneyUnit.Satoshi)
            };
        }



        public async Task<string> GetOmniTransaction(string transactionId)
        {
            var request = new RPCRequest("omni_gettransaction", new object[] { transactionId });
            var response = await Client.SendCommandAsync(request);
            var json = response.Result;
            return json.ToString();
        }

        public FeeRate GetMinRelayTxFee()
        {
            var resp = Client.SendCommand(RPCOperations.getnetworkinfo);
            var jResult = ((JObject)resp.Result);
            var relayTxFee = jResult["relayfee"].Value<decimal>();
            return new FeeRate(new Money(relayTxFee, MoneyUnit.BTC));
        }

        public Money GetP2PKHDustThreadhold()
        {
            var output = new TxOut(Money.Parse("0"), BitcoinAddress.Create("mjparMCvVPaUYmQWuWtCdygRKN3vaSRvjP", Network.TestNet));
            var minRelayTxFee = this.GetMinRelayTxFee();
            return output.GetDustThreshold(minRelayTxFee);
        }

    }
}
