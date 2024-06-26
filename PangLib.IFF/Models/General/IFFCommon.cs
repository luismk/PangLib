using System.IO;
using System;
using System.Runtime.InteropServices;
using PangLib.IFF.Models.Flags;
using PangLib.IFF.Extensions;

namespace PangLib.IFF.Models.General
{
    /// <summary>
    /// Ref's:
    /// my code first: https://github.com/oung/Py_Source_JP/tree/master/Src/PangyaFileCore
    ///<code></code>
    /// replace: https://github.com/Acrisio-Filho/SuperSS-Dev/blob/master/Server%20Lib/Projeto%20IOCP/TYPE/data_iff.h
    /// update in 30/06/2023 - 10:40 AM by LuisMK
    ///<code></code>
    /// Common data structure found at the head of many IFF datasets
    ///<code></code>
    /// Size 192 bytes    ?
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public partial class IFFCommon : ICloneable
    {
        //------------------- IFF BASIC ----------------------------\\
        public uint Active { get; set; }//0 start position
        public uint ID { get; set; }//4 start position
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Name { get; set; }//8 start position
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 1)]
        public IFFLevel Level { get; set; }//72 start position
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 43)]
        public string Icon { get; set; }//73 start position
        //--------------------------end--------------------------------\\

        //------------------ SHOP DADOS ---------------------------------\\
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
        public IFFShopData Shop { get; set; } = new IFFShopData();
        //-------------------  END  ------------------------------\\
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 24)]
        public IFFTikiShopData tiki { get; set; } = new IFFTikiShopData();
        //-------------------- TIME IFF--------------\\
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 36)]
        public IFFDate date { get; set; } = new IFFDate();
        /// <summary>
        /// voce pode carregar qualquer iff(que contem o Base)
        /// </summary>
        /// <param name="reader">binario de leitura</param>
        /// <param name="LenghtStr">tamanho do string name</param>
        public void Load(ref PangyaBinaryReader reader, uint LenghtStr, long recordLength = 0, uint version = 11, bool jump = false)
        {
            //------------------- IFF BASIC ----------------------------\\
            Active = reader.ReadUInt32();
            ID = reader.ReadUInt32();
            Name = reader.ReadPStr(LenghtStr);
            Level = new IFFLevel
            {
                level = reader.ReadByte() //49 start position
            };
            Icon = reader.ReadPStr(43); //89 start position
            //--------------------------end--------------------------------\\
            //------------------ SHOP DADOS ---------------------------------\\
            Shop = (IFFShopData)reader.Read(new IFFShopData(), 16);
            //-------------------  END  ------------------------------\\
            //------------------ Tiki SHOP---------------------\\
            if (version != 11)
            {
                tiki = (IFFTikiShopData)reader.Read(new IFFTikiShopData(), 24);
            }
            //-----------------------------------------------\\

            //-------------------- TIME IFF--------------\\
            date = (IFFDate)reader.Read(new IFFDate(), 36);
            //--------------------------------------------------\\
            if (jump)
            {
                reader.BaseStream.Seek(8L + (recordLength), SeekOrigin.Begin);
            }
        }
        /// <summary>
        /// Envia uma notificao ao Editor/Dev 
        /// voce n�o pode listar este item pois o valor ira 
        /// ativar um codigo no ProjectG de alerta
        /// </summary>
        public bool SendMessage()
        {
            bool result = Shop.flag_shop.can_send_mail_and_personal_shop
             || Shop.flag_shop.block_mail_and_personal_shop
             || Shop.flag_shop.is_saleable;
            if (result && Shop.Price >= 1000000)
            {
              new  Exception($"\nBe careful, you activated an item, but did not change its value\n check this item({ID})");
                return true;
            }
            return false;
               
        }


        public string GetItemName()
        {
            return Name;
        }
        public int ShopFlag
        {
            get { return (int)(Shop == null ? 0 : Shop.flag_shop.ShopFlag); }
            set
            {
                Shop.flag_shop.ShopFlag = (ShopFlag)value;
            }
        }
        public int MoneyFlag
        {
            get { return (int)(Shop == null ? 0 : Shop.flag_shop.MoneyFlag); }
            set
            {
                Shop.flag_shop.MoneyFlag = (MoneyFlag)value;
            }
        }
        public uint Price
        {
            get => Shop == null ? 0 : Shop.Price;
            set => Shop.Price = value;
        }
        public byte ItemLevel
        {
            get => (byte)(Level == null ? 0 : Level.level);
            set => Level.level = value;
        }
        public uint DiscountPrice
        {
            get => Shop == null ? 0 : Shop.DiscountPrice;
            set => Shop.DiscountPrice = value;
        }
        public uint isValid
        {
            get => Active;
            set => Active = value;
        }
        public sbyte GetShopPriceType()
        {
            return (sbyte)Shop.flag_shop.ShopFlag;
        }

        public bool IsBuyable()
        {
            if (Active == 1 && Shop.flag_shop.MoneyFlag == 0 || (int)Shop.flag_shop.MoneyFlag == 1 || (int)Shop.flag_shop.MoneyFlag == 2)
            {
                return true;
            }
            return false;
        }

        public bool IsNormal()
        {
            return Active == 1 && Shop.flag_shop.IsNormal || Shop.flag_shop.is_saleable;
        }
        public bool IsExist()
        {
            return Convert.ToBoolean(Active);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        public IFFCommon()
        {
            Name = "[NEW ITEM] by LuisMK";
            Icon = "Icon.tga";
            date = new IFFDate();
            tiki = new IFFTikiShopData();
            Shop = new IFFShopData();
        }
        //conversion this 
        public virtual IFFCommon CreateNewItem()
        {
            Name = "[NEW ITEM] by LuisMK";
            Icon = "[NEW ICON] by LuisMK";
            date = new IFFDate();
            tiki = new IFFTikiShopData();
            Shop = new IFFShopData();
            return this;
        }

        public uint TypeItem()
        {
            return (uint)(int)Math.Round((ID & 0xFC000000) / Math.Pow(2.0, 26.0));
        }

        public bool IsDupItem()
        {
            return (Active == 1 && Shop.flag_shop.IsDuplication);
        }

        public bool IsSellItem()
        {
            return (Active == 1 && Shop.flag_shop.is_saleable);
        }

        public bool IsGiftItem()
        {
            // � saleable ou giftable nunca os 2 juntos por que � a flag composta Somente Purchase(compra)
            // ent�o fa�o o xor nas 2 flag se der o valor de 1 � por que ela � um item que pode presentear
            // Ex: 1 + 1 = 2 N�o �
            // Ex: 1 + 0 = 1 OK
            // Ex: 0 + 1 = 1 OK
            // Ex: 0 + 0 = 0 N�o �
            byte is_giftable = Convert.ToByte(Shop.flag_shop.IsGift);
            byte _is_saleable = Convert.ToByte(Shop.flag_shop.is_saleable);
            return (Active == 1 && Shop.flag_shop.IsCash
                    && (_is_saleable ^ is_giftable) == 1);
        }

        public bool IsOnlyDisplay()
        {
            return (Active == 1 && Shop.flag_shop.IsDisplay);
        }

        public bool IsOnlyPurchase()
        {
            return (Active == 1 && Shop.flag_shop.is_saleable
                    && Shop.flag_shop.IsGift);
        }

        public bool IsOnlyGift()
        {
            return (Active == 1 && Shop.flag_shop.IsCash
                    && Shop.flag_shop.is_saleable && Shop.flag_shop.IsGift == false);
        }

        public bool IsPSQ()
        {
            return (Active == 1 && Shop.flag_shop.can_send_mail_and_personal_shop || Shop.flag_shop.IsPSQ || Shop.flag_shop.IsTradeable);
        }
        /// <summary>
        /// verifica � pang, cookie ou esta oculto dentro do shopping
        /// </summary>
        /// <returns>0= cookies, 1= pang, 2= hide </returns>
        public int GetTypeCash()
        {
            //se testar o flag do tipo de moeda antes, n�o vai dar certo
            //tem que testar o flag hide primeiro
            var result = Shop.flag_shop.IsHide ? 0 : Shop.flag_shop.IsCash ? 1 : 2;
            return result;
        }

        public void SetItemNew(bool tipoMoeda)
        {
            SetFlagShop(tipoMoeda, true);
        }

        public void SetItemHot(bool tipoMoeda)
        {
            SetFlagShop(tipoMoeda, false, true);
        }

        public void SetItemNormal(bool tipoMoeda)
        {
            SetFlagShop(tipoMoeda, false, false, true);
        }

        public void SetItemPSQ(bool tipoMoeda)
        {
            SetFlagShop(tipoMoeda, false, false, false, true);
        }

        public void SetItemGift(bool tipoMoeda)
        {
            SetFlagShop(tipoMoeda, false, false, false, false, false, false, true);
        }

        public void SetItemDesativado(bool tipoMoeda)
        {
            SetFlagShop(tipoMoeda, false, false, false, false, false, true);
        }

        public void SetItemDisplay(bool tipoMoeda)
        {
            SetFlagShop(tipoMoeda, false, false, false, false, true);
        }

        /// <summary>
        /// seta tipo de bandeira a ser exibida no shopFlag
        /// </summary>
        /// <param name="tipoMoney">false � pang, true � cookies</param>
        /// <param name="IsNew">novo item</param>
        /// <param name="IsHot">item quente</param>
        /// <param name="IsNormal"> item normal</param>
        /// <param name="IsPSQ"> item personal shop</param>
        /// <param name="IsGift">item gift</param>
        /// <param name="IsDesativado">desativado</param>
        /// <param name="Is_OnlyDisplay">� item para ficar visivel </param>
        public void SetFlagShop(bool tipoMoeda /*pang = false*/, bool IsNew = false, bool IsHot = false, bool IsNormal = false, bool IsPSQ = false, bool IsDesativado = false, bool Is_OnlyDisplay = false, bool IsGift = false, bool IsSpecial = false)
        {
            if (Is_OnlyDisplay)
            {
                Shop.flag_shop.ShopFlag = Flags.ShopFlag.Only_Display;
                Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
            }
            else if (IsDesativado)
            {
                Shop.flag_shop.ShopFlag = Flags.ShopFlag.None;
                Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
            }
            else
            {
                if (!tipoMoeda && IsNormal) // pangs
                {
                    if (IsPSQ && IsGift) // combinacao dos 3(ativo no shop normal e tambem no psq)
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Combine98;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                    }
                    else if (IsGift) // combinacao dos 3(ativo no shop normal e tambem no psq)
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Combine96;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.BannerNew;
                    }
                    else if (IsPSQ) // combinacao dos 3(ativo no shop normal e tambem no psq)
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Unknown64;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                    }
                    else
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Pang;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                    }
                }
                else if (tipoMoeda && IsNormal) // cookies
                {
                    if (IsPSQ && IsGift) // nesse quesito tenho que olhar pro setitem, e o caddieitem
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Combine99;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                    }
                    else if (IsGift) // combinacao dos 3(ativo no shop normal e tambem no psq)
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Combine;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                    }
                    else if (IsPSQ) // combinacao dos 3(ativo no shop normal e tambem no psq)
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Cookies_0;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                    }
                    else if (IsNew) // normal +new is cookies
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Cookies_0;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.Active;
                    }
                    else if (IsHot) // normal + hot teste ainda
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Cookies_0;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.BannerNew;
                    }
                    else // somente no shop, sem flags, � cookies
                    {
                        Shop.flag_shop.ShopFlag = Flags.ShopFlag.Cookies_0;
                        Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                    }
                }

                if (IsNew && (!tipoMoeda || (tipoMoeda && IsNormal)))
                {
                    Shop.flag_shop.ShopFlag = Flags.ShopFlag.Coupon;
                    Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.Active;
                }

                if (IsHot)
                {
                    Shop.flag_shop.ShopFlag = Flags.ShopFlag.Pang;
                    Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.Flag2;
                }

                if (IsPSQ)
                {
                    Shop.flag_shop.ShopFlag = Flags.ShopFlag.NonGiftable;
                    Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.None;
                }

                if (IsSpecial)
                {
                    Shop.flag_shop.ShopFlag = Flags.ShopFlag.Pang;
                    Shop.flag_shop.MoneyFlag = Flags.MoneyFlag.Unknown3;
                }
            }

            SendMessage();
        }
    }
}
