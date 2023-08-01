import { writable } from 'svelte/store';
import { toastStore } from '@skeletonlabs/skeleton';
import type { ToastSettings } from '@skeletonlabs/skeleton';

export interface notificationType {
	type?: notificationTypes;
	message: string;
}

export interface notificationStoreType {
	type: notificationTypes;
	message: string;
	btnStyle: string;
}

export enum notificationTypes {
	success,
	warning,
	error,
	surface
}

function createNotificationStore() {
	// set Store Type
	const { subscribe, set, update } = writable<notificationStoreType>();

	return {
		//pass Store default functions
		subscribe,
		set,
		update,
		setNotification: (notification: notificationType) => {
			notification.type =
				notification.type === undefined ? notificationTypes.surface : notification.type;
			let btnStyle: string;

			switch (notification.type) {
				case notificationTypes.success:
					btnStyle = 'btn-icon btn-icon-sm variant-filled-success shadow-md';
					break;
				case notificationTypes.warning:
					btnStyle = 'btn-icon btn-icon-sm variant-filled-warning shadow-md';
					break;
				case notificationTypes.error:
					btnStyle = 'btn-icon btn-icon-sm variant-filled-error shadow-md';
					break;
				case notificationTypes.surface:
					btnStyle = 'btn-icon btn-icon-sm variant-filled-surface shadow-md';
					break;
			}

			notificationStore.set({
				type: notification.type,
				message: notification.message,
				btnStyle: btnStyle
			});
			notificationStore.subscribe((value) => {
			//console.log('notificationStore.setNotification()',value);
			});
		},

		triggerNotification: () => {
			let timeout: number = 30000;
				let classes: string = "";
				let message: string = "";
			notificationStore.subscribe((value) => {
				

				switch (value.type) {
					case notificationTypes.success:
						classes =
							'bg-success-300 border-solid border-2 border-success-500 shadow-md text-surface-900';
						break;
					case notificationTypes.warning:
						classes =
							'bg-warning-300 border-solid border-2 border-warning-500 shadow-md text-surface-900';
						break;
					case notificationTypes.error:
						classes =
							'bg-error-300 border-solid border-2 border-error-500 shadow-md text-surface-900';
						break;
					case notificationTypes.surface:
						classes =
							'bg-surface-300 border-solid border-2 border-surface-500 shadow-md text-surface-900';
						break;
				}

				message = value.message;
				//console.log('notificationStore.triggerNotification()',value);
			});
			if(classes != "" && message !="")
			{
			const notificationToast: ToastSettings = {
				classes: classes,
				message: message,
				timeout: timeout
			};
			toastStore.trigger(notificationToast);
		}
		},

		clear: () => {
			toastStore.clear();
		},

		showNotification: (notification: notificationType) => {
			notificationStore.clear();
			notificationStore.setNotification({ type: notification.type, message: notification.message });
			notificationStore.triggerNotification();
		},

		getBtnStyle: () => {
			let btnStyle: string = '';
			notificationStore.subscribe((value) => {
			do{			
					if (value === undefined || value.btnStyle === undefined) {
						notificationStore.setNotification({message:''});
					}
					else
					{
						btnStyle = value.btnStyle;
					}			
			}while(btnStyle === '')
			//console.log('notificationStore.getBtnStyle()',value);
				});
			return btnStyle;
		}
	};
}

//crate and export the instance of the store
export const notificationStore = createNotificationStore();
