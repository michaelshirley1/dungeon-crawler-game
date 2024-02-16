using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallwayData : MonoBehaviour
{   
    [System.Serializable]
    public struct Config
    {
        public bool PosX;
        public bool PosY;  
        public bool NegX;  
        public bool NegY;  

        public Config(bool posy, bool posx, bool negy, bool negx)
        {
            PosX = posx;
            PosY = posy;
            NegX = negx;
            NegY = negy;
        }

        public readonly int GetNumberOfConnectors()
        {
            return Convert.ToInt32(PosX) + Convert.ToInt32(PosY) + 
                   Convert.ToInt32(NegX) + Convert.ToInt32(NegY);
        }

        public readonly Vector2Int GetNumConnectorsAxis()
        {
            return new Vector2Int(
                Convert.ToInt32(PosX) + Convert.ToInt32(NegX),
                Convert.ToInt32(PosY) + Convert.ToInt32(NegY)
            );
        }
    }

    public enum HallwayType
    {
        UNSET, NONE, END, CORNER, STRAIGHT, THREEWAY, FOURWAY
    }

    private HallwayType _Type = HallwayType.UNSET;

    public Vector3 Size;
    public Config HallwayConfiguration;

    public HallwayType GetHallwayType()
    {
        if (_Type == HallwayType.UNSET)
        {
            _Type = ParseConfig(HallwayConfiguration);
        }

        return _Type;
    }
    

    private HallwayType ParseConfig(Config config)
    {
        HallwayType hallwayType = HallwayType.UNSET;
        int numConnectors = config.GetNumberOfConnectors();
        Vector2Int numConnectorsAxis = config.GetNumConnectorsAxis();
        switch(numConnectors)
        {
            case 0: hallwayType = HallwayType.NONE; break;
            case 1: hallwayType = HallwayType.END; break;
            case 2: 
                if (numConnectorsAxis.x == 2 || numConnectorsAxis.y == 2)
                {
                    hallwayType = HallwayType.STRAIGHT;
                } 
                else
                {
                    hallwayType = HallwayType.CORNER;
                }
                break;
            case 3: hallwayType = HallwayType.THREEWAY; break;
            case 4: hallwayType = HallwayType.FOURWAY; break;
        }

        return hallwayType;
    }


    public bool IsEqual(Config other)
    {
        HallwayType otherType = ParseConfig(other);
        _Type = ParseConfig(HallwayConfiguration);

        return otherType == _Type;
    }


    public bool GetPositionOffsetAndRotation(Config config, out Vector3 offset, out Quaternion rotation)
    {
        offset = Vector3.zero;
        rotation = Quaternion.identity;
        if (IsEqual(config))
        {
            HallwayType otherConfigType = ParseConfig(config);

            if (otherConfigType == HallwayType.FOURWAY || otherConfigType == HallwayType.NONE)
            {
                return true;
            }

            if(otherConfigType == HallwayType.STRAIGHT) 
            {
                if (config.PosX != HallwayConfiguration.PosX ||
                    config.PosY != HallwayConfiguration.PosY ||
                    config.NegX != HallwayConfiguration.NegX ||
                    config.NegY != HallwayConfiguration.NegY)
                {
                    offset = new Vector3(Size.x, 0, 0);
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                }
            }
            else
            {
                if (config.PosX == HallwayConfiguration.PosY &&
                    config.PosY == HallwayConfiguration.NegX &&
                    config.NegX == HallwayConfiguration.NegY &&
                    config.NegY == HallwayConfiguration.PosX)
                {
                    offset = new Vector3(0, 0, Size.z);
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                }
                
                if (config.PosX == HallwayConfiguration.NegX &&
                            config.PosY == HallwayConfiguration.NegY &&
                            config.NegX == HallwayConfiguration.PosX &&
                            config.NegY == HallwayConfiguration.PosY)
                {
                    offset = new Vector3(Size.x, 0, Size.z);
                    rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                
                if (config.PosX == HallwayConfiguration.NegY &&
                            config.PosY == HallwayConfiguration.PosX &&
                            config.NegX == HallwayConfiguration.PosY &&
                            config.NegY == HallwayConfiguration.NegX)
                {
                    offset = new Vector3(Size.x, 0, 0);
                    rotation = Quaternion.Euler(0f, 270f, 0f);
                }
            }
            
            return true;
        }

        return false;
    }
    
}


