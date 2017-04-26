using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Untari.TIA
{
    public class TIAAddress
    {
        // TIA Addresses written by the CPU and read by TIA
        public const int VSYNC = 0x00;      // vertical sync set/clear
        public const int VBLANK = 0x01;     // vertical blank set/clear
        public const int WSYNC = 0x02;      // wait for leading edge of horizontal blank
        public const int RSYNC = 0x03;      // reset horizontal sync counter

        // These registers control how wide a player or missile graphic is to be drawn.
        //
        // Bits 5 & 4 (clocks controls the width)
        //      00 = 1 clock
        //      01 = 2 clocks
        //      10 = 4 clocks
        //      11 = 8 clocks
        //
        // Bits 2,1,0 controls the number of copies and their spacing
        // Half the drawing area is 80 clocks. With 8 clocks per square there are 9 possible drawing locations
        //
        //      000 = X-------- one copy
        //      001 = X-X------ two copies (close to each other)
        //      010 = X---X---- two copies (medium spacing)
        //      011 = X-X-X---- three copies (close spacing)
        //      100 = X-------X two copies (wide spacing)
        //      101 = XX------- double size
        //      110 = X---X---X three copies (medium spacing)
        //      111 = XXXX----- quad size
        public const int NUSIZ0 = 0x04;     // number-size player-missle 0
        public const int NUSIZ1 = 0x05;     // number-size player-missle 1

        // These registers determine the color and luminosity of the given objects
        public const int COLUP0 = 0x06;     // color-lum player 0 and missile 0
        public const int COLUP1 = 0x07;     // color-lum player 1 and missile 1
        public const int COLUPF = 0x08;     // color-lum playfield and ball
        public const int COLUBK = 0x09;     // color-lum background

        public const int REFP0 = 0x0b;      // reflect player 0
        public const int REFP1 = 0x0c;      // reflect player 1

        // This register is to create a playfield of walls and barriers that rarely move.
        // This draws the left half only.  Right half is drawn either as a duplication or reflection.
        // The PF register is 20 bits long. PF0 only uses bits 0-3.
        // If the bit=1 the playfield color is drawn, if bit=0 the background color is drawn
        public const int PF0 = 0x0d;        // playfield register byte 0
        public const int PF1 = 0x0e;        // playfield register byte 1
        public const int PF2 = 0x0f;        // playfield register byte 2

        // This register controls the right half drawing of the play field.
        // A "0" in bit 0 is a duplication, a "1" in bit 0 is a reflection.
        // This register controls the size of the ball graphics.
        // Bits 5 and 4 are used to determine 1,2,4, or 8 clocks (same as above table)
        public const int CTRLPF = 0x0a;     // control playfield ball size & collisions


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

        // Player Graphics. 
        public const int GRP0 = 0x1b;       // graphics player 0
        public const int GRP1 = 0x1c;       // graphics player 1

        // Enable/Disable the drawing of missile graphics with a "0" or "1" in bit 1.
        public const int ENAM0 = 0x1d;      // graphics enable missile 0
        public const int ENAM1 = 0x1e;      // graphics enable missile 1

        // Enable/Disable the drawing of the ball with a "0" or "1" in bit 1
        public const int ENABL = 0x1f;      // graphics enable ball
        public const int HMP0 = 0x20;       // horizontal motion player 0
        public const int HMP1 = 0x21;       // horizontal motion player 1
        public const int HMM0 = 0x22;       // horizontal motion missile 0
        public const int HMM1 = 0x23;       // horizontal motion missile 1
        public const int HMBL = 0x24;       // horizontal motion ball
        public const int VDELP0 = 0x25;     // vertical delay player 0
        public const int VDELP1 = 0x26;     // vertical delay player 1

        // Write a "1" in bit 0 to delay the drawing of the ball by one line
        public const int VDELBL = 0x27;     // vertical delay ball

        public const int RESMP0 = 0x28;     // reset missle 0 to player 0
        public const int RESMP1 = 0x29;     // reset missle 1 to player 1
        public const int HMOVE = 0x2a;      // apply horizontal motion
        public const int HMCLR = 0x2b;      // clear horizontal motion registers
        public const int CXCLR = 0x2c;      // clear collision latches

        // TIA addresses read by the CPU
        // collision registers use only bit 7 and bit 6
        // P0=player0, P1=player1, M0=missile0, M1=missile1, BL=ball, PF=playfield
        public const int CXM0P = 0x00;      // collision (M0/P1 and M0/P0)
        public const int CXM1P = 0x01;      // collision (M1/P0 and M1/P1)
        public const int CXP0FB = 0x02;     // collision (P0/PF and P0/BL)
        public const int CXP1FB = 0x03;     // collision (P1/PF and P1/BL)
        public const int CXM0FB = 0x04;     // collision (M0/PF and M0/BL)
        public const int CXM1FB = 0x05;     // collision (M1/PF and M1/BL)
        public const int CXBLPF = 0x06;     // collision (BL/PF) bit 7 only
        public const int CXPPMM = 0x07;     // collision (P0/P1 and M0/M1)
        // uses bit 7 only
        public const int INPT0 = 0x08;      // read pot port
        public const int INPT1 = 0x09;      // read pot port
        public const int INPT2 = 0x0a;      // read pot port
        public const int INPT3 = 0x0b;      // read pot port
        public const int INPT4 = 0x0c;      // read input
        public const int INPT5 = 0x0d;      // read input
    }
}
