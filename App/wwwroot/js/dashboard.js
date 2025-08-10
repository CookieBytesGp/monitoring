// Dashboard charts initialization

document.addEventListener('DOMContentLoaded', function () {
    // Live data refresh for dashboard stats
    function refreshDashboardStats() {
        fetch('/Home/GetLiveStats')
            .then(response => response.json())
            .then(data => {
                // Update dashboard statistics using the correct selectors
                const activeCamerasValue = document.querySelector('.stat-card:nth-child(1) .stat-value');
                const todayEventsValue = document.querySelector('.stat-card:nth-child(2) .stat-value');
                const unacknowledgedValue = document.querySelector('.stat-card:nth-child(3) .stat-value');
                const systemHealthValue = document.querySelector('.stat-card:nth-child(4) .stat-value');
                
                if (activeCamerasValue) activeCamerasValue.innerText = data.activeCameras;
                if (todayEventsValue) todayEventsValue.innerText = data.recentEvents;
                if (unacknowledgedValue) unacknowledgedValue.innerText = data.unacknowledgedEvents;
                
                // Update system health status if needed
                if (systemHealthValue) {
                    if (data.systemErrors > 0) {
                        systemHealthValue.innerText = 'Warning';
                        systemHealthValue.classList.add('text-warning');
                        systemHealthValue.classList.remove('text-success');
                    } else {
                        systemHealthValue.innerText = 'Good';
                        systemHealthValue.classList.add('text-success');
                        systemHealthValue.classList.remove('text-warning');
                    }
                }
            })
            .catch(error => console.error('Error fetching dashboard stats:', error));
    }
    
    // Refresh stats every 30 seconds
    setInterval(refreshDashboardStats, 30000);
    
    // Camera stream functionality
    window.viewStream = function(streamUrl) {
        // Create and show camera stream modal
        const modal = document.createElement('div');
        modal.className = 'stream-modal';
        modal.innerHTML = `
            <div class="stream-modal-content">
                <span class="close-modal">&times;</span>
                <h4>Live Camera Stream</h4>
                <div class="stream-container">
                    <img src="${streamUrl}" alt="Camera Stream" onerror="this.src='/images/no-stream.png'">
                </div>
            </div>
        `;
        document.body.appendChild(modal);
        
        // Close modal functionality
        const closeBtn = modal.querySelector('.close-modal');
        closeBtn.onclick = function() {
            document.body.removeChild(modal);
        }
        
        // Close modal when clicking outside
        modal.onclick = function(event) {
            if (event.target === modal) {
                document.body.removeChild(modal);
            }
        }
    };
    
    // Initialize dashboard on page load
    refreshDashboardStats();
});
