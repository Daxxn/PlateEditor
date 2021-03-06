﻿using MVVMLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PlateEditorWPF
{
   public enum PlateType
   {
      Approach = 0,
      ArrivalDeparture = 1,
      AirportDiagram = 2,
   };

   public class PlateMetaData : Model
   {
      #region - Fields & Properties
      public static readonly string[] AllRegions = new string[]
      {
         "AK", "CN",
         "EC1", "EC2", "EC3",
         "NC1", "NC2", "NC3",
         "NE1", "NE2", "NE3", "NE4",
         "NW1",
         "SC1", "SC2", "SC3", "SC4", "SC5",
         "SE1", "SE2", "SE3", "SE4",
         "SW1", "SW2", "SW3", "SW4",
      };
      public static readonly Dictionary<string, string> ApproachTypes = new Dictionary<string, string>
      {
         { "NA-", "NA-" },
         { "RNAV (GPS)", "RNAV" },
         { "RNAV (RNP)", "RNAVRNP" },
         { "LOC", "LOC" },
         { "VOR", "VOR" },
         { "ILS", "ILS" },
         { "NDB", "NDB" },
         { "VOR (GPS)", "VORGPS" },
         { "TACAN", "TACAN" },
         { "ILS or LOC", "ILSLOC" },
         { "VOR or GPS", "VORGPS" },
         { "Hi ILS/LOC", "HiILSLOC" },
         { "Hi TACAN", "HiTACAN" },
         { "VOR or TACAN", "VORTACAN" },
         { "VOR/DME", "VORDME" },
         { "NDB/DME", "NDBDME" },
         { "Hi RNAV", "HiRNAV" },
         { "Hi VOR/TACAN", "HiVORTACAN" },
         { "LDA/DME", "LDADME" },
         { "ILS PRM", "ILSPRM" },
         { "RNAV (GPS) PRM", "RNAVGPSPRM" },
         { "ILS or LOC/DME", "ILSLOCDME" },
         { "Hi VOR DME TACAN", "HiVORDMETACAN" },
         { "VOR/DME or TACAN", "VORDMETACAN" },
         { "LOC/DME", "LOCDME" },
         { "LDA/PRM", "LDAPRM" },
         { "VISUAL", "VIS" },
      };

      private string _IATACode;
      private PlateType _type;
      private string _regionCode;
      private string _approachType;
      private string _runway;
      private string _approachOption;
      private string _other;

      private Uri _plateFile;
      private bool _savePlate;
      #endregion

      #region - Constructors
      public PlateMetaData() { }
      #endregion

      #region - Methods
      public static PlateMetaData ParseName(string name)
      {
         try
         {
            string[] nameSplit = name.Split(' ', StringSplitOptions.None);
            if (nameSplit.Length == 7)
            {
               var apType = nameSplit[3];
               return new PlateMetaData
               {
                  IATACode = nameSplit[0],
                  RegionCode = nameSplit[1],
                  Type = Enum.Parse<PlateType>(nameSplit[2]),
                  //ApproachType = ApproachTypes.ContainsKey(nameSplit[3]) ? ApproachTypes[nameSplit[3]] : "NA-",
                  ApproachType = apType,
                  Runway = nameSplit[4],
                  ApproachOption = nameSplit[5],
                  Other = nameSplit[6],
               };
            }
            else
            {
               throw new ArgumentException($"Name parse failed. The names length ({nameSplit.Length}) doesn match the metadata properties. (7)");
            }
         }
         catch (Exception)
         {
            throw;
         }
      }

      public static List<PlateMetaData> BuildMetaData(string[] filePaths)
      {
         List<PlateMetaData> output = new List<PlateMetaData>();
         foreach (var platePath in filePaths)
         {
            var plate = ParseName(Path.GetFileNameWithoutExtension(platePath));
            plate.PlateFile = new Uri(platePath);
            output.Add(plate);
         }
         return output;
      }

      public void Save(string saveDir)
      {
         if (WillSavePlate)
         {
            File.Copy(
                  PlateFile.LocalPath,
                  Path.Combine(saveDir, FileName),
                  true
               );
         }
      }

      public string GetApproachTypeKey(string apType)
      {
         if (ApproachTypes.ContainsValue(apType))
         {
            return ApproachTypes.First(kv => kv.Value == apType).Key;
         }
         return "NA-";
      }

      public void GetApproachType(string apType)
      {
         if (ApproachTypes.ContainsValue(apType))
         {
            ApproachType = ApproachTypes.First(kv => kv.Value == apType).Value;
         }
      }

      public override string ToString()
      {
         return $"{IATACode} {RegionCode} {Type} {ApproachType} {Runway} {ApproachOption} {Other}";
      }
      #endregion

      #region - Full Properties
      public string IATACode
      {
         get { return _IATACode; }
         set
         {
            _IATACode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public PlateType Type
      {
         get { return _type; }
         set
         {
            _type = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public string RegionCode
      {
         get { return _regionCode; }
         set
         {
            _regionCode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public string ApproachType
      {
         get { return _approachType; }
         set
         {
            _approachType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public string Runway
      {
         get { return _runway; }
         set
         {
            _runway = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public string ApproachOption
      {
         get { return _approachOption; }
         set
         {
            _approachOption = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public string Other
      {
         get { return _other; }
         set
         {
            _other = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public Uri PlateFile
      {
         get { return _plateFile; }
         set
         {
            _plateFile = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(WillSavePlate));
         }
      }

      public string FileName
      {
         get
         {
            return $"{ToString()}.png";
         }
      }

      public bool IsNameChanged
      {
         get
         {
            if (PlateFile is null)
            {
               return false;
            }
            return Path.GetFileNameWithoutExtension(PlateFile.LocalPath) != ToString();
         }
      }


      public bool WillSavePlate
      {
         get { return _savePlate; }
         set
         {
            _savePlate = value;
            OnPropertyChanged();
         }
      }
      #endregion
   }
}
