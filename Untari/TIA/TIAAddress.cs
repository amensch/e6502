using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untari.TIA
{
    public class TIAAddress
    {
        // Write Addresses
        public const int VSYNC = 0x00;      // vertical sync set/clear
        public const int VBLANK = 0x01;     // vertical blank set/clear
        public const int WSYNC = 0x02;      // wait for leading edge of horizontal blank
        public const int RSYNC = 0x03;      // reset horizontal sync counter
        public const int NUSIZ0 = 0x04;     // number-size player-missle 0
        public const int NUSIZ1 = 0x05;     // number-size player-missle 1
        public const int COLUP0 = 0x06;     // color-lum player 0
        public const int COLUP1 = 0x07;     // color-lum player 1
        public const int COLUPF = 0x08;     // color-lum playfield
        public const int COLUBK = 0x09;     // color-lum background
        public const int CTRLPF = 0x0a;     // control playfield ball size & collisions
        public const int REFP0 = 0x0b;      // reflect player 0
        public const int REFP1 = 0x0c;      // reflect player 1
        public const int PF0 = 0x0d;        // playfield register byte 0
        public const int PF1 = 0x0e;        // playfield register byte 1
        public const int PF2 = 0x0f;        // playfield register byte 2
        public const int RESP0 = 0x10;      // reset player 0
        public const int RESP1 = 0x11;      // reset player 1
        public const int RESM0 = 0x12;      // reset missile 0
        public const int RESM1 = 0x13;      // reset missile 1
        public const int RESBL = 0x14;      // reset ball
        public const int AUDC0 = 0x15;      // audio control 0
        public const int AUDC1 = 0x16;      // audio control 1
        public const int AUDF0 = 0x17;      // audio frequency 0
        public const int AUDF1 = 0x18;      // audio frequency 1
        public const int AUDV0 = 0x19;      // audio volume 0
        public const int AUDV1 = 0x1a;      // audio volume 1
        public const int GRP0 = 0x1b;       // graphics player 0
        public const int GRP1 = 0x1c;       // graphics player 1
        public const int ENAM0 = 0x1d;      // graphics enable missle 0
        public const int ENAM1 = 0x1e;      // graphics enable missle 1
        public const int ENABL = 0x1f;      // graphics enable ball
        public const int HMP0 = 0x20;       // horizontal motion player 0
        public const int HMP1 = 0x21;       // horizontal motion player 1
        public const int HMM0 = 0x22;       // horizontal motion missile 0
        public const int HMM1 = 0x23;       // horizontal motion missile 1
        public const int HMBL = 0x24;       // horizontal motion ball
        public const int VDELP0 = 0x25;     // vertical delay player 0
        public const int VDELP1 = 0x26;     // vertical delay player 1
        public const int VDELBL = 0x27;     // vertical delay ball
        public const int RESMP0 = 0x28;     // reset missle 0 to player 0
        public const int RESMP1 = 0x29;     // reset missle 1 to player 1
        public const int HMOVE = 0x2a;      // apply horizontal motion
        public const int HMCLR = 0x2b;      // clear horizontal motion registers
        public const int CXCLR = 0x2c;      // clear collision latches

        // Read Addresses
        public const int CXM0P = 0x00;
        public const int CXM1P = 0x01;
        public const int CXP0FB = 0x02;
        public const int CXP1FB = 0x03;
        public const int CXM0FB = 0x04;
        public const int CXM1FB = 0x05;
        public const int CXBLPF = 0x06;
        public const int CXPPMM = 0x07;
        public const int INPT0 = 0x08;
        public const int INPT1 = 0x09;
        public const int INPT2 = 0x0a;
        public const int INPT3 = 0x0b;
        public const int INPT4 = 0x0c;
        public const int INPT5 = 0x0d;
    }
}
