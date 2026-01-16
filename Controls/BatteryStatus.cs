using System;
using System.Drawing;
using System.Windows.Forms;
using MissionPlanner;

namespace MissionPlanner.Controls
{
    public partial class BatteryStatus : Form
    {
        Label lblBattTitle = new Label();
        Label lblVolt = new Label();
        Label lblCurrent = new Label();
        Label lblSpeed = new Label();
        PictureBox pbBattery = new PictureBox(); // 배터리 그림통
        Timer updateTimer = new Timer();
        Label lblBattPercent = new Label();

        public BatteryStatus()
        {
            // 기본 창 설정
            this.Size = new Size(350, 250);
            this.Text = "Battery & Speed Status";
            this.BackColor = Color.Black;
            this.TopMost = true;

            SetupControls();

            updateTimer.Interval = 500;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            this.FormClosing += (s, e) => updateTimer.Stop();
        }



        private void SetupControls()
        {
            this.Size = new Size(450, 280);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // 배터리 뭉치 기준 X 좌표
            int batteryX = 280;
            int batteryWidth = 100;

            // 1. "잔여배터리" 제목 (크기 줄임 + 맨 앞으로 가져오기)
            lblBattTitle.Text = "잔여배터리";
            lblBattTitle.Location = new Point(batteryX, 20); // Y좌표 살짝 위로
            lblBattTitle.Size = new Size(batteryWidth, 25);
            lblBattTitle.Font = new Font("맑은 고딕", 10, FontStyle.Bold); // 11 -> 10으로 축소
            lblBattTitle.ForeColor = Color.White;
            lblBattTitle.BackColor = Color.Transparent; // 배경 투명
            lblBattTitle.TextAlign = ContentAlignment.MiddleCenter;

            if (!this.Controls.Contains(lblBattTitle)) this.Controls.Add(lblBattTitle);
            lblBattTitle.BringToFront(); // [핵심] 그림 뒤로 숨지 않게 설정

            // 2. 배터리 도화지
            pbBattery.Location = new Point(batteryX, 50);
            pbBattery.Size = new Size(batteryWidth, 130);
            pbBattery.BackColor = Color.Transparent;

            pbBattery.Paint -= PbBattery_Paint;
            pbBattery.Paint += PbBattery_Paint;

            if (!this.Controls.Contains(pbBattery)) this.Controls.Add(pbBattery);

            // 3. 배터리 퍼센트 %
            lblBattPercent.Location = new Point(batteryX, 185);
            lblBattPercent.Size = new Size(batteryWidth, 35);
            lblBattPercent.Font = new Font("Consolas", 18, FontStyle.Bold);
            lblBattPercent.BackColor = Color.Transparent;
            lblBattPercent.TextAlign = ContentAlignment.MiddleCenter;

            if (!this.Controls.Contains(lblBattPercent)) this.Controls.Add(lblBattPercent);
            lblBattPercent.BringToFront();

            // 4. 왼쪽 텍스트 그룹 (배터리와 더 가깝게 너비 조정)
            Font hanguFont = new Font("맑은 고딕", 17, FontStyle.Bold);

            lblVolt.Location = new Point(20, 45);
            lblVolt.Size = new Size(250, 35);
            lblVolt.Font = hanguFont;
            this.Controls.Add(lblVolt);

            lblCurrent.Location = new Point(20, 80);
            lblCurrent.Size = new Size(250, 35);
            lblCurrent.Font = hanguFont;
            this.Controls.Add(lblCurrent);

            lblSpeed.Location = new Point(20, 145);
            lblSpeed.Size = new Size(250, 80);
            lblSpeed.Font = new Font("맑은 고딕", 15, FontStyle.Bold);
            this.Controls.Add(lblSpeed);
        }

        private void PbBattery_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 하얀색 외곽선 (Pen 두께 4)
            using (Pen whitePen = new Pen(Color.White, 4))
            {
                // 도화지 너비 100 기준 중앙 배치 (X=20)
                g.DrawRectangle(whitePen, 20, 25, 60, 100);
                // 단자 중앙 배치 (X=35)
                g.DrawRectangle(whitePen, 35, 5, 30, 18);
            }

            // 내부 채우기 (데이터 연동)
            var cs = MainV2.comPort.MAV.cs;
            double remaining = (cs != null) ? cs.battery_remaining : 0;

            Color fillColor = (remaining <= 20) ? Color.Red : (remaining <= 50 ? Color.Orange : Color.Lime);
            int fillHeight = (int)(92 * (Math.Max(2, remaining) / 100.0));

            if (fillHeight > 0)
            {
                // 몸체 내부(X=24, 너비 53)에 맞춰 채우기
                int yPos = 121 - fillHeight;
                using (SolidBrush brush = new SolidBrush(fillColor))
                {
                    g.FillRectangle(brush, 24, yPos, 53, fillHeight);
                }
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var cs = MainV2.comPort.MAV.cs;
            if (cs == null) return;

            double remaining = cs.battery_remaining;

            // 텍스트 업데이트
            lblVolt.Text = $"전압: {cs.battery_voltage:F2} V";
            lblCurrent.Text = $"전류: {cs.current:F1} A";
            lblBattPercent.Text = $"{(int)remaining}%";
            lblSpeed.Text = $"대기속도: {cs.airspeed:F1} m/s\n지면속도: {cs.groundspeed:F1} m/s";

            // 색상 강제 고정 (매 틱마다 다시 설정)
            lblVolt.ForeColor = Color.Lime;
            lblCurrent.ForeColor = Color.Lime;
            lblSpeed.ForeColor = Color.Cyan;

            // 퍼센트 색상 고정 (흰색 방지)
            if (remaining <= 25) lblBattPercent.ForeColor = Color.Red;
            else if (remaining <= 50) lblBattPercent.ForeColor = Color.Orange;
            else lblBattPercent.ForeColor = Color.White; // 기본은 흰색, 필요시 Color.Lime 등으로 변경 가능

            pbBattery.Invalidate(); // 그림 새로고침
        }
    }
}